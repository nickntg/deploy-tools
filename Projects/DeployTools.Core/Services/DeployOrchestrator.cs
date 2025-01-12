using System;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Context.Interfaces;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using NLog;

namespace DeployTools.Core.Services
{
    public class DeployOrchestrator(IDbContext dbContext, ICoreSsh ssh) : IDeployOrchestrator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private string _deployId;

        public async Task DeployAsync(Application application, Host host)
        {
            var package = await dbContext.PackagesRepository.GetByIdAsync(application.PackageId);

            application.Port = host.NextFreePort;

            host.NextFreePort++;

            await dbContext.HostsRepository.UpdateAsync(host);
            await dbContext.ApplicationsRepository.UpdateAsync(application);

            var deploy = new ApplicationDeploy
            {
                ApplicationId = application.Id,
                HostId = host.Id,
                IsCompleted = false
            };

            await dbContext.ApplicationDeploysRepository.SaveAsync(deploy);

            _deployId = deploy.Id;

            ssh.JournalEvent += SshOnJournalEvent;

            var deployFolder = GetDeployFolder(host.SshUserName, application.Name);
            var serviceName = GetServiceName(application.Name);
            var serviceLocation = GetServiceLocation(serviceName);

            var applicationCreateSuccessful = true;

            // TODO: Port belongs at active deployment instead of application.
            Logger.Info($"Starting deployment of application {application.Name} with id {application.Id}, package {package.Name} with id {package.Id}, to host {host.Address} with id {host.Id}, at port {application.Port}");

            try
            {
                Logger.Info("Connecting to host");
                if (!await Connect(host))
                {
                    Logger.Error("Connection failed");
                    return;
                }

                Logger.Info("Uploading package");
                var result = await ssh.UploadDirectoryAsync(package.DeployableLocation, deployFolder);
                if (!result.IsSuccessful)
                {
                    Logger.Error("Upload of package failed");
                    throw new Exception("Upload of package failed");
                }

                await RunCommandWithLogging($"chmod +x {deployFolder}/{package.ExecutableFile}",
                    "Making package application executable");

                var serviceContents =
                    "[Unit]\r\nDescription={kestrel_name}\r\nAfter=network.target\r\n\r\n[Service]\r\nWorkingDirectory={deploy_folder}\r\nExecStart={deploy_executable}\r\nRestart=always\r\n# User and group under which the service runs\r\nUser=ec2-user\r\nGroup=ec2-user\r\nEnvironment=DOTNET_ENVIRONMENT=Production\r\nEnvironment=ASPNETCORE_URLS=http://0.0.0.0:{listening_port}\r\n\r\n[Install]\r\nWantedBy=multi-user.target";

                serviceContents = serviceContents
                    .Replace("{deploy_folder}", deployFolder)
                    .Replace("{deploy_executable}", $"{deployFolder}/{package.ExecutableFile}")
                    .Replace("{kestrel_name}", $"Application {application.Name}, package {package.Name}")
                    .Replace("{listening_port}", application.Port.ToString());

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

                await dbContext.ActiveDeploymentsRepository.CleanupDeploymentsOfApplicationAsync(application.Id);

                await dbContext.ActiveDeploymentsRepository.SaveAsync(new ActiveDeployment
                {
                    PackageId = application.PackageId,
                    ApplicationId = application.Id,
                    HostId = host.Id,
                    DeployId = deploy.Id
                });

                /*
                 * Next steps:
                 *  - Add a new target group for the new application and port.
                 *  - Add the target group to the load balancer.
                 *  - Add a rule matching an incoming host name to the newly created target group.
                 *  - Clean up as necessary if something fails.
                 */
            }
            catch (Exception ex)
            {
                applicationCreateSuccessful = false;

                Logger.Error(ex, "Application creation failed - rolling back changes");

                await TakeDownAsync(application, host, _deployId);
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

                await TakeDownAsync(application, host, deployment.DeployId);
            }
        }

        private async Task TakeDownAsync(Application application, Host host, string deployId)
        {
            var deployFolder = GetDeployFolder(host.SshUserName, application.Name);
            var serviceName = GetServiceName(application.Name);
            var serviceLocation = GetServiceLocation(serviceName);

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

        private string GetDeployFolder(string sshUserName, string applicationName)
        {
            return $"/home/{sshUserName}/{applicationName}";
        }

        private string GetServiceName(string applicationName)
        {
            return $"{applicationName}.service";
        }

        private string GetServiceLocation(string serviceName)
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
    }
}
