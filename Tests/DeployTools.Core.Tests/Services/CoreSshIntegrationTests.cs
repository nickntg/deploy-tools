using DeployTools.Core.Services;
using Xunit;
using Xunit.Abstractions;

namespace DeployTools.Core.Tests.Services
{
    public class CoreSshIntegrationTests(ITestOutputHelper testOutputHelper)
    {
        [Fact]
        public async Task Connect()
        {
            var o = new CoreSsh();
            var result = await o.Connect("3.73.118.204", "ec2-user", "E:/UserData/Nick/Secured/ntg-main.pem");
        }

        [Fact]
        public async Task Execute()
        {
            var o = new CoreSsh();
            await o.Connect("3.73.118.204", "ec2-user", "E:/UserData/Nick/Secured/ntg-main.pem");

            var result = await o.RunCommand("ls");
        }

        [Fact]
        public async Task Upload()
        {
            var o = new CoreSsh();
            await o.Connect("3.73.118.204", "ec2-user", "E:/UserData/Nick/Secured/ntg-main.pem");

            var result =
                await o.UploadFile("D:/aws/sampleapp/WebApplication1/bin/Release/net8.0/publish/appsettings.json",
                    "/home/ec2-user/appsettings.json");
        }

        [Fact]
        public async Task UploadDirectory()
        {
            var o = new CoreSsh();

            o.UploadDirectoryProgressEvent += (sender, args) =>
            {
                testOutputHelper.WriteLine($"Files {args.FilesCompleted}/{args.FilesTotal}");
                testOutputHelper.WriteLine($"Bytes {args.BytesCompleted}/{args.BytesTotal}");
            };

            await o.Connect("3.73.118.204", "ec2-user", "E:/UserData/Nick/Secured/ntg-main.pem");

            var result =
                await o.UploadDirectory("D:/aws/sampleapp/WebApplication1/bin/Release/net8.0/publish",
                    "/home/ec2-user/test1");

            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public async Task FileExists()
        {
            var o = new CoreSsh();

            await o.Connect("3.79.209.195", "ec2-user", "E:/UserData/Nick/Secured/ntg-main.pem");

            var result = await o.FileExists("/home/ec2-user/index.html");
            Assert.True(result.IsSuccessful);
            Assert.True(result.ListingFound);
        }

        [Fact]
        public async Task DirectoryExists()
        {
            var o = new CoreSsh();

            await o.Connect("3.79.209.195", "ec2-user", "E:/UserData/Nick/Secured/ntg-main.pem");

            var result = await o.DirectoryExists("/home/ec2-user");
            Assert.True(result.IsSuccessful);
            Assert.True(result.ListingFound);
        }

        [Fact]
        public async Task OrchestrateUploadAndServiceStart()
        {
            var projectName = "test2";
            var projectFolder = "D:/aws/sampleapp/WebApplication1/bin/Release/net8.0/publish";
            var projectExecutable = "WebApplication1";
            var dirToDeploy = "/home/ec2-user";

            var o = new CoreSsh();

            o.JournalEvent += (sender, args) =>
            {
                testOutputHelper.WriteLine($"{args.CommandExecuted}, started {args.CommandStarted:yyyy-MM-dd HH:mm:ss}, completed {args.CommandCompleted:yyyy-MM-dd HH:mm:ss}, success: {args.WasSuccessful}");
            };

            await o.Connect("18.153.102.215", "ec2-user", "E:/UserData/Nick/Secured/ntg-main.pem");

            var deployFolder = $"{dirToDeploy}/{projectName}";
            var serviceName = $"{projectName}.service";
            var serviceLocation = $"/etc/systemd/system/{serviceName}";

            try
            {
                var result = await o.UploadDirectory(projectFolder, deployFolder);
                Assert.True(result.IsSuccessful);

                result = await o.RunCommand($"chmod +x {deployFolder}/{projectExecutable}");
                Assert.True(result.IsSuccessful);

                var serviceContents =
                    "[Unit]\r\nDescription=My Kestrel-based Web App\r\nAfter=network.target\r\n\r\n[Service]\r\nWorkingDirectory={deploy_folder}\r\nExecStart={deploy_executable}\r\nRestart=always\r\n# User and group under which the service runs\r\nUser=ec2-user\r\nGroup=ec2-user\r\nEnvironment=DOTNET_ENVIRONMENT=Production\r\n\r\n[Install]\r\nWantedBy=multi-user.target";

                serviceContents = serviceContents
                    .Replace("{deploy_folder}", deployFolder)
                    .Replace("{deploy_executable}", $"{deployFolder}/{projectExecutable}");

                result = await o.UploadContent(serviceContents, $"{deployFolder}/{serviceName}");
                Assert.True(result.IsSuccessful);

                result = await o.RunCommand($"sudo mv {deployFolder}/{serviceName} {serviceLocation}");
                Assert.True(result.IsSuccessful);

                result = await o.RunCommand("sudo systemctl daemon-reload");
                Assert.True(result.IsSuccessful);

                result = await o.RunCommand($"sudo systemctl enable {serviceName}");
                Assert.True(result.IsSuccessful);

                result = await o.RunCommand($"sudo systemctl start {serviceName}");
                Assert.True(result.IsSuccessful);

                result = await o.RunCommand($"sudo systemctl status {serviceName}");
                Assert.True(result.IsSuccessful);
            }
            catch (Exception ex)
            {
                var result = await o.FileExists(serviceLocation);
                Assert.True(result.IsSuccessful);

                if (result.ListingFound)
                {
                    result = await o.RunCommand($"sudo systemctl stop {serviceName}");
                    Assert.True(result.IsSuccessful);

                    result = await o.RunCommand($"sudo rm {serviceLocation}");
                    Assert.True(result.IsSuccessful);

                    result = await o.RunCommand("sudo systemctl daemon-reload");
                    Assert.True(result.IsSuccessful);
                }

                result = await o.DirectoryExists(deployFolder);
                Assert.True(result.IsSuccessful);

                if (result.ListingFound)
                {
                    result = await o.RunCommand($"sudo rm -r -f {deployFolder}");
                    Assert.True(result.IsSuccessful);
                }
            }

            Thread.Sleep(500);
        }
    }
}
