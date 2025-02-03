﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.ElasticLoadBalancingV2;
using Amazon.ElasticLoadBalancingV2.Model;
using Amazon.RDS;
using Amazon.RDS.Model;
using DeployTools.Core.DataAccess.Context.Interfaces;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Helpers;
using DeployTools.Core.Services.Interfaces;
using NLog;

namespace DeployTools.Core.Services
{
    public class DeployOrchestrator(IDbContext dbContext, 
        ICoreSsh ssh, 
        IAmazonElasticLoadBalancingV2 elbClient, 
        IAmazonRDS rdsClient) : IDeployOrchestrator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private string _deployId;

        public async Task DeployAsync(Application application, Host host)
        {
            var package = await dbContext.PackagesRepository.GetByIdAsync(application.PackageId);

            var deployPort = host.NextFreePort;

            host.NextFreePort++;

            await dbContext.HostsRepository.UpdateAsync(host);

            var deploy = new ApplicationDeploy
            {
                ApplicationId = application.Id,
                HostId = host.Id,
                IsCompleted = false
            };

            await dbContext.ApplicationDeploysRepository.SaveAsync(deploy);

            _deployId = deploy.Id;

            ssh.JournalEvent += SshOnJournalEvent;

            var deployFolder = ConstructDeployFolder(host.SshUserName, application.Name);
            var serviceName = ConstructServiceName(application.Name);
            var serviceLocation = ConstructServiceLocation(serviceName);

            var applicationCreateSuccessful = true;

            Logger.Info($"Starting deployment of application {application.Name} with id {application.Id}, package {package.Name} with id {package.Id}, to host {host.Address} with id {host.Id}, at port {deployPort}");

            DatabaseConfiguration dbInfo = null;

            try
            {
                dbInfo = await CreateDatabaseAsync(application.Name, application.RdsPackageId);

                Logger.Info("Connecting to host");
                if (!await Connect(host))
                {
                    Logger.Error("Connection failed");
                    return;
                }

                Logger.Info("Uploading package");

                var configFileEndsWith = string.Empty;
                var replaceMap = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(dbInfo.DbArn))
                {
                    configFileEndsWith = ".Production.json";
                    replaceMap.Add("{user_id}", dbInfo.DbUserName);
                    replaceMap.Add("{user_password}", dbInfo.DbPassword);
                    replaceMap.Add("{db_host}", dbInfo.Address);
                    replaceMap.Add("{db_port}", dbInfo.Port.ToString());
                    replaceMap.Add("{db_name}", dbInfo.DbName);
                }

                var result = await ssh.UploadDirectoryAsync(package.DeployableLocation, deployFolder, configFileEndsWith, replaceMap);
                if (!result.IsSuccessful)
                {
                    Logger.Error("Upload of package failed");
                    throw new Exception("Upload of package failed");
                }

                await RunCommandWithLogging($"chmod +x {deployFolder}/{package.ExecutableFile}",
                    "Making package application executable");

                var serviceContents =
                    "[Unit]\r\nDescription={kestrel_name}\r\nAfter=network.target\r\n\r\n[Service]\r\nWorkingDirectory={deploy_folder}\r\nExecStart={deploy_executable}\r\nRestart=always\r\n# User and group under which the service runs\r\nUser={user}\r\nGroup={user}\r\nEnvironment=DOTNET_ENVIRONMENT=Production\r\nEnvironment=ASPNETCORE_URLS=http://0.0.0.0:{listening_port}\r\n\r\n[Install]\r\nWantedBy=multi-user.target";

                serviceContents = serviceContents
                    .Replace("{deploy_folder}", deployFolder)
                    .Replace("{deploy_executable}", $"{deployFolder}/{package.ExecutableFile}")
                    .Replace("{kestrel_name}", $"Application {application.Name}, package {package.Name}")
                    .Replace("{listening_port}", deployPort.ToString())
                    .Replace("{user}", host.SshUserName);

                Logger.Info("Uploading service file");
                result = await ssh.UploadContentAsync(serviceContents, $"{deployFolder}/{serviceName}");
                if (!result.IsSuccessful)
                {
                    Logger.Error("Service file upload failed");
                    throw new Exception("Failed to upload the service file");
                }

                await RunCommandWithLogging($"sudo mv {deployFolder}/{serviceName} {serviceLocation}", "Moving service file");

                await RunCommandWithLogging("sudo systemctl daemon-reload", "Reloading daemons");

                await RunCommandWithLogging($"sudo systemctl enable {serviceName}", "Enabling service for boot");

                await RunCommandWithLogging($"sudo systemctl start {serviceName}", "Starting service");

                await RunCommandWithLogging($"sudo systemctl status {serviceName}", "Getting service status");

                await CreateLoadBalancingAsync(application, host, deployPort);

                await dbContext.ActiveDeploymentsRepository.CleanupDeploymentsOfApplicationAsync(application.Id);

                await dbContext.ActiveDeploymentsRepository.SaveAsync(new ActiveDeployment
                {
                    PackageId = application.PackageId,
                    ApplicationId = application.Id,
                    HostId = host.Id,
                    DeployId = deploy.Id,
                    Port = deployPort,
                    RdsArn = dbInfo.DbArn
                });
            }
            catch (Exception ex)
            {
                applicationCreateSuccessful = false;

                Logger.Error(ex, "Application creation failed - rolling back changes");

                await TakeDownAsync(application, host, _deployId, dbInfo?.DbArn);
            }
            finally
            {
                ssh.JournalEvent -= SshOnJournalEvent;

                await ssh.DisconnectAsync();

                deploy.IsSuccessful = applicationCreateSuccessful;
                deploy.IsCompleted = true;

                await dbContext.ApplicationDeploysRepository.UpdateAsync(deploy);
            }
        }

        public async Task TakeDownAsync(Application application)
        {
            var activeDeployments =
                await dbContext.ActiveDeploymentsRepository.GetDeploymentsOfApplicationAsync(application.Id);

            foreach (var deployment in activeDeployments)
            {
                var host = await dbContext.HostsRepository.GetByIdAsync(deployment.HostId);

                await TakeDownAsync(application, host, deployment.DeployId, deployment.RdsArn);
            }
        }

        private void EmitJournalEntry(JournalEventArgs entry, string command)
        {
            entry.CommandCompleted = DateTimeOffset.UtcNow;
            entry.CommandExecuted = command;
            entry.WasSuccessful = true;

            SshOnJournalEvent(this, entry);
        }

        private async Task<DatabaseConfiguration> CreateDatabaseAsync(string applicationName, string rdsPackageId)
        {
            if (string.IsNullOrEmpty(rdsPackageId))
            {
                return new DatabaseConfiguration();
            }

            Logger.Info($"Retrieving RDS package with id {rdsPackageId}");

            var rdsPackage = await dbContext.RdsPackagesRepository.GetByIdAsync(rdsPackageId);
            
            if (rdsPackage is null)
            {
                Logger.Error($"RDS package with id {rdsPackageId} not found");
                throw new InvalidOperationException("RDS package not found");
            }

            var dbConfiguration = new DatabaseConfiguration
            {
                DbName = RandomExtensions.GenerateDatabaseName(),
                DbPassword = 32.GenerateRandomPassword(),
                DbUserName = "master"
            };

            var journalEntry = new JournalEventArgs
            {
                CommandStarted = DateTimeOffset.UtcNow,
            };

            var createRequest = new CreateDBInstanceRequest
            {
                DBInstanceIdentifier = ConstructDbInstanceName(applicationName),
                Engine = rdsPackage.Engine,
                EngineVersion = rdsPackage.EngineVersion,
                MasterUsername = dbConfiguration.DbUserName,
                MasterUserPassword = dbConfiguration.DbPassword,
                DBInstanceClass = rdsPackage.DbInstance,
                VpcSecurityGroupIds = [rdsPackage.VpcSecurityGroupId],
                PubliclyAccessible = false,
                MultiAZ = false,
                BackupRetentionPeriod = 7,
                StorageEncrypted = true,
                DeletionProtection = false,
                DBSubnetGroupName = rdsPackage.DbSubnetGroupName,
                DBName = dbConfiguration.DbName,
                MonitoringInterval = 0
            };

            if (!string.IsNullOrEmpty(rdsPackage.StorageType))
            {
                createRequest.StorageType = rdsPackage.StorageType;
                createRequest.AllocatedStorage = rdsPackage.StorageInGigabytes ?? 20;
                createRequest.MaxAllocatedStorage = rdsPackage.StorageInGigabytes ?? 20;
            }

            try
            {
                var response = await rdsClient.CreateDBInstanceAsync(createRequest);

                dbConfiguration.DbArn = response.DBInstance.DBInstanceArn;

                EmitJournalEntry(journalEntry, $"RDS instance {dbConfiguration.DbArn} created");

                var startedTime = DateTimeOffset.UtcNow;

                while (DateTimeOffset.UtcNow.Subtract(startedTime).TotalSeconds < 5*60)
                {
                    Logger.Info($"Waiting for RDS with arn {dbConfiguration.DbArn} to get address info");
                    Thread.Sleep(5000);

                    var describeResponse = await rdsClient.DescribeDBInstancesAsync(new DescribeDBInstancesRequest
                    {
                        DBInstanceIdentifier = dbConfiguration.DbArn
                    });

                    var instance = describeResponse.DBInstances[0];

                    if (instance.Endpoint is not null)
                    {
                        dbConfiguration.Address = instance.Endpoint.Address;
                        dbConfiguration.Port = instance.Endpoint.Port;

                        journalEntry.CommandStarted = DateTimeOffset.UtcNow;
                        EmitJournalEntry(journalEntry, $"RDS instance {dbConfiguration.DbArn} created and configured, host={dbConfiguration.Address}, port={dbConfiguration.Port}, db={dbConfiguration.DbName}, user={dbConfiguration.DbUserName}, password={dbConfiguration.DbPassword}");

                        break;
                    }
                }

                if (string.IsNullOrEmpty(dbConfiguration.Address))
                {
                    Logger.Error($"Could not get address info from RDS instance {dbConfiguration.DbArn} - timeout exceeded");
                    throw new InvalidOperationException("Could not get RDS address info");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"RDS creation failed, RDS package id is {rdsPackageId}");

                await TakeDownDatabaseAsync(dbConfiguration.DbArn);

                throw;
            }

            return dbConfiguration;
        }

        private async Task CreateLoadBalancingAsync(Application application, Host host, int deployPort)
        {
            var targetGroupName = ConstructTargetGroupName(application.Name);
            var ruleName = ConstructRuleName(application.Name);

            var journal = new JournalEventArgs
            {
                CommandStarted = DateTimeOffset.UtcNow
            };
            Logger.Info($"Creating target group {targetGroupName}");
            var targetGroupResponse = await elbClient.CreateTargetGroupAsync(new CreateTargetGroupRequest
            {
                Name = targetGroupName,
                Protocol = ProtocolEnum.HTTP,
                Port = deployPort,
                VpcId = host.VpcId,
                TargetType = TargetTypeEnum.Instance,
                HealthCheckEnabled = true,
                HealthCheckPath = "/",
                HealthCheckIntervalSeconds = 30,
                HealthCheckTimeoutSeconds = 5,
                HealthyThresholdCount = 3,
                UnhealthyThresholdCount = 3
            });

            EmitJournalEntry(journal, $"Create target group {targetGroupName}");

            var targetGroup = targetGroupResponse.TargetGroups[0];

            journal.CommandStarted = DateTimeOffset.UtcNow;
            Logger.Info($"Registering instance {host.InstanceId} to target group {targetGroupName}");
            await elbClient.RegisterTargetsAsync(new RegisterTargetsRequest
            {
                TargetGroupArn = targetGroup.TargetGroupArn,
                Targets = [new() { Id = host.InstanceId }]
            });

            EmitJournalEntry(journal, $"Register instance {host.InstanceId} to target group {targetGroupName}");

            journal.CommandStarted = DateTimeOffset.UtcNow;
            Logger.Info("Retrieving listeners");
            var listeners = await elbClient.DescribeListenersAsync(new DescribeListenersRequest
            {
                LoadBalancerArn = host.AssignedLoadBalancerArn
            });

            EmitJournalEntry(journal, "Retrieve listeners");

            var portToFind = listeners.Listeners.Count == 1
                ? 80
                : 443;

            Logger.Info($"Will look for listener on port {portToFind}");

            foreach (var listener in listeners.Listeners)
            {
                if (listener.Port != portToFind)
                {
                    continue;
                }

                journal.CommandStarted = DateTimeOffset.UtcNow;
                Logger.Info($"Retrieving all rules of listener {listener.ListenerArn} to determine next priority");
                var listRulesResponse = await elbClient.DescribeRulesAsync(new DescribeRulesRequest
                {
                    ListenerArn = listener.ListenerArn
                });
                EmitJournalEntry(journal, $"List rules of listener {listener.ListenerArn}");

                var priority = 0;

                if (listRulesResponse is not null && listRulesResponse.Rules.Count > 0)
                {
                    var numeric = listRulesResponse.Rules.Where(x => int.TryParse(x.Priority, out _)).ToList();
                    if (numeric.Count > 0)
                    {
                        priority = int.Parse(numeric.Max(x => x.Priority));
                    }
                }

                priority++;

                journal.CommandStarted = DateTimeOffset.UtcNow;
                Logger.Info($"Adding rule to listener - domain {application.Domain}");
                await elbClient.CreateRuleAsync(new CreateRuleRequest
                {
                    Tags =
                    [
                        new()
                        {
                            Key = "Name",
                            Value = ruleName
                        }
                    ],
                    ListenerArn = listener.ListenerArn,
                    Priority = priority,
                    Actions =
                    [
                        new()
                        {
                            TargetGroupArn = targetGroup.TargetGroupArn,
                            Type = ActionTypeEnum.Forward
                        }
                    ],
                    Conditions =
                    [
                        new()
                        {
                            Field = "host-header",
                            HostHeaderConfig = new HostHeaderConditionConfig
                            {
                                Values = [application.Domain, $"*.{application.Domain}"]
                            }
                        }
                    ]
                });

                EmitJournalEntry(journal, $"Add rule to listener for domain {application.Domain}");
            }
        }

        private async Task TakeDownDatabaseAsync(string rdsArn)
        {
            if (string.IsNullOrEmpty(rdsArn))
            {
                return;
            }

            Logger.Info($"Starting takedown of RDS database with arn {rdsArn}");

            var journalEntry = new JournalEventArgs
            {
                CommandStarted = DateTimeOffset.UtcNow
            };

            var dbInstance = await rdsClient.DescribeDBInstancesAsync(new DescribeDBInstancesRequest
            {
                DBInstanceIdentifier = rdsArn
            });

            await rdsClient.DeleteDBInstanceAsync(new DeleteDBInstanceRequest
            {
                DBInstanceIdentifier = dbInstance.DBInstances[0].DBInstanceIdentifier,
                DeleteAutomatedBackups = true,
                SkipFinalSnapshot = true
            });

            EmitJournalEntry(journalEntry, $"RDS instance {rdsArn} deleted");

            Logger.Info($"RDS instance with arn {rdsArn} deleted");
        }

        private async Task TakeDownLoadBalancingAsync(Application application, Host host)
        {
            Logger.Info($"Starting takedown of load balancing of application {application.Name} with id {application.Id}, from host {host.Address} with id {host.Id}");

            var targetGroupName = ConstructTargetGroupName(application.Name);

            var journal = new JournalEventArgs
            {
                CommandStarted = DateTimeOffset.UtcNow
            };
            Logger.Info("Retrieving target groups of load balancers");
            var results = await elbClient.DescribeTargetGroupsAsync(new DescribeTargetGroupsRequest
            {
                LoadBalancerArn = host.AssignedLoadBalancerArn
            });

            EmitJournalEntry(journal, "Retrieve target groups of load balancers");

            Logger.Info($"Trying to find target group {targetGroupName}");
            var foundTargetGroup = results.TargetGroups.FirstOrDefault(x => x.TargetGroupName.Equals(targetGroupName));

            if (foundTargetGroup is not null)
            {
                journal.CommandStarted = DateTimeOffset.UtcNow;
                Logger.Info("Listing listeners");
                var listeners = await elbClient.DescribeListenersAsync(new DescribeListenersRequest
                {
                    LoadBalancerArn = host.AssignedLoadBalancerArn
                });

                EmitJournalEntry(journal, "Listing listeners");

                foreach (var listener in listeners.Listeners)
                {
                    var rule = ConstructRuleName(application.Name);
                    
                    journal.CommandStarted = DateTimeOffset.UtcNow;
                    Logger.Info($"Trying to find rule {rule} in listener {listener.ListenerArn}");
                    var rules = await elbClient.DescribeRulesAsync(new DescribeRulesRequest
                    {
                        ListenerArn = listener.ListenerArn
                    });

                    EmitJournalEntry(journal, $"Trying to find rule {rule} in listener {listener.ListenerArn}");

                    var foundRule = rules.Rules.FirstOrDefault(x =>
                        x.Actions[0].TargetGroupArn.Equals(foundTargetGroup.TargetGroupArn));

                    if (foundRule is not null)
                    {
                        journal.CommandStarted = DateTimeOffset.UtcNow;
                        Logger.Info($"Removing rule {foundRule.RuleArn}");
                        await elbClient.DeleteRuleAsync(new DeleteRuleRequest
                        {
                            RuleArn = foundRule.RuleArn
                        });

                        EmitJournalEntry(journal, $"Remove rule {foundRule.RuleArn}");
                    }
                }

                journal.CommandStarted = DateTimeOffset.UtcNow;
                Logger.Info($"Removing target group {foundTargetGroup.TargetGroupArn}");
                await elbClient.DeleteTargetGroupAsync(new DeleteTargetGroupRequest
                {
                    TargetGroupArn = foundTargetGroup.TargetGroupArn
                });

                EmitJournalEntry(journal, $"Remove target group {foundTargetGroup.TargetGroupArn}");
            }
            else
            {
                // Target group was not attached to the load balancer. We need to search all target groups.
                journal.CommandStarted = DateTimeOffset.UtcNow;
                Logger.Info("Retrieving all target groups");
                results = await elbClient.DescribeTargetGroupsAsync(new DescribeTargetGroupsRequest());

                EmitJournalEntry(journal, "Retrieve all target groups");

                foundTargetGroup = results.TargetGroups.FirstOrDefault(x => x.TargetGroupName.Equals(targetGroupName));

                if (foundTargetGroup is not null)
                {
                    journal.CommandStarted = DateTimeOffset.UtcNow;
                    Logger.Info($"Removing target group {foundTargetGroup.TargetGroupArn}");
                    await elbClient.DeleteTargetGroupAsync(new DeleteTargetGroupRequest
                    {
                        TargetGroupArn = foundTargetGroup.TargetGroupArn
                    });

                    EmitJournalEntry(journal, $"Remove target group {foundTargetGroup.TargetGroupArn}");
                }
                else
                {
                    Logger.Warn($"Target group named {targetGroupName} not found");
                }
            }
        }

        private async Task TakeDownAsync(Application application, Host host, string deployId, string rdsArn)
        {
            var deployFolder = ConstructDeployFolder(host.SshUserName, application.Name);
            var serviceName = ConstructServiceName(application.Name);
            var serviceLocation = ConstructServiceLocation(serviceName);

            var wasConnected = true;

            Logger.Info($"Starting takedown of application {application.Name} with id {application.Id}, from host {host.Address} with id {host.Id}, deploy id {deployId}");

            try
            {
                if (!await ssh.IsConnectedAsync())
                {
                    wasConnected = false;

                    ssh.JournalEvent += SshOnJournalEvent;

                    _deployId = deployId;

                    Logger.Info("Connecting to host");
                    if (!await Connect(host))
                    {
                        Logger.Error("Connection failed");
                        return;
                    }
                }

                await TakeDownDatabaseAsync(rdsArn);

                await TakeDownLoadBalancingAsync(application, host);
                
                Logger.Info("Checking for service file");
                var result = await ssh.FileExistsAsync(serviceLocation);
                if (!result.IsSuccessful)
                {
                    Logger.Error("Could not check for service file - MANUAL CLEANUP REQUIRED");
                    return;
                }

                if (result.ListingFound)
                {
                    await RunCommandWithLogging($"sudo systemctl stop {serviceName}", "Stopping service", false);

                    await RunCommandWithLogging($"sudo rm {serviceLocation}", "Removing service file", false);

                    await RunCommandWithLogging("sudo systemctl daemon-reload", "Reloading daemons", false);
                }

                Logger.Info("Checking for deployable existence");
                result = await ssh.DirectoryExistsAsync(deployFolder);
                if (!result.IsSuccessful)
                {
                    Logger.Error("Could not check for deployable existence - MANUAL CLEANUP REQUIRED");
                    return;
                }

                if (result.ListingFound)
                {
                    await RunCommandWithLogging($"sudo rm -r -f {deployFolder}", "Reloading daemons", false);
                }
            }
            finally
            {
                if (!wasConnected)
                {
                    ssh.JournalEvent -= SshOnJournalEvent;

                    await ssh.DisconnectAsync();

                    await dbContext.ActiveDeploymentsRepository.CleanupDeploymentsOfApplicationAsync(application.Id);
                }
            }

        }

        private static string ConstructDbInstanceName(string applicationName)
        {
            return $"{applicationName
                .Replace(" ", "_")
                .Replace("_", "-")}-db-instance";
        }

        private static string ConstructTargetGroupName(string applicationName)
        {
            return $"{applicationName
                .Replace(" ", "_")
                .Replace("_", "-")}-target-group";
        }

        private static string ConstructRuleName(string applicationName)
        {
            return $"{applicationName
                .Replace(" ", "_")
                .Replace("_", "-")}-rule";
        }

        private static string ConstructDeployFolder(string sshUserName, string applicationName)
        {
            return $"/home/{sshUserName}/{applicationName}";
        }

        private static string ConstructServiceName(string applicationName)
        {
            return $"{applicationName}.service";
        }

        private static string ConstructServiceLocation(string serviceName)
        {
            return $"/etc/systemd/system/{serviceName}";
        }

        private async Task RunCommandWithLogging(string command, string log, bool throwException = true)
        {
            Logger.Debug(log);

            var result = await ssh.RunCommandAsync(command);

            if (!result.IsSuccessful)
            {
                Logger.Error($"Failed - {log}");

                if (throwException)
                {
                    throw new Exception($"Failed - {log}");
                }
            }
        }

        private async Task<bool> Connect(Host host)
        {
            var result = await ssh.ConnectAsync(host.Address, host.SshUserName, host.KeyFile);

            return result.IsSuccessful;
        }

        private void SshOnJournalEvent(object sender, JournalEventArgs e)
        {
            Logger.Debug($"Command {e.CommandExecuted}, started {e.CommandStarted:yyyy-MM-dd HH:mm:ss}, ended {e.CommandCompleted:yyyy-MM-dd HH:mm:ss}, success={e.WasSuccessful}");

            dbContext.JournalEntriesRepository.SaveAsync(new JournalEntry
            {
                CommandCompleted = e.CommandCompleted,
                CommandExecuted = e.CommandExecuted,
                CommandStarted = e.CommandStarted,
                DeployId = _deployId,
                Output = e.Output,
                WasSuccessful = e.WasSuccessful
            }).Wait();
        }

        internal class DatabaseConfiguration
        {
            public string DbName { get; set; }
            public string DbUserName { get; set; }
            public string DbPassword { get; set; }
            public string Address { get; set; }
            public int Port { get; set; }
            public string DbArn { get; set; }
        }
    }
}

