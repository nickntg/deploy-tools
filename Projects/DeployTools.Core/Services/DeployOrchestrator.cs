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

            var deployFolder = $"/home/{host.SshUserName}/{application.Name}";
            var serviceName = $"{application.Name}.service";
            var serviceLocation = $"/etc/systemd/system/{serviceName}";

            var applicationCreateSuccessful = true;

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
                    "[Unit]\r\nDescription={kestrel_name}\r\nAfter=network.target\r\n\r\n[Service]\r\nWorkingDirectory={deploy_folder}\r\nExecStart={deploy_executable}\r\nRestart=always\r\n# User and group under which the service runs\r\nUser=ec2-user\r\nGroup=ec2-user\r\nEnvironment=DOTNET_ENVIRONMENT=Production\r\n\r\n[Install]\r\nWantedBy=multi-user.target";

                serviceContents = serviceContents
                    .Replace("{deploy_folder}", deployFolder)
                    .Replace("{deploy_executable}", $"{deployFolder}/{package.ExecutableFile}")
                    .Replace("{kestrel_name}", $"Application {application.Name}, package {package.Name}");

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
            }
            catch (Exception ex)
            {
                applicationCreateSuccessful = false;

                Logger.Error(ex, "Application creation failed - rolling back changes");

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
                ssh.JournalEvent -= SshOnJournalEvent;

                deploy.IsSuccessful = applicationCreateSuccessful;
                deploy.IsCompleted = true;

                await dbContext.ApplicationDeploysRepository.UpdateAsync(deploy);
            }
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
