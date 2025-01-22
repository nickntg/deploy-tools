using Amazon.ElasticLoadBalancingV2;
using Amazon.ElasticLoadBalancingV2.Model;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Models;
using DeployTools.Core.Services;
using DeployTools.Core.Services.Interfaces;
using DeployTools.Core.Tests.Helpers;
using FakeItEasy;
using Xunit;
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace DeployTools.Core.Tests.Services
{
    public class DeployOrchestratorTests
    {
        [Fact]
        public async Task DeployHappyPath()
        {
            var dbContext = DbHelpers.New();

            A.CallTo(() => dbContext.PackagesRepository.GetByIdAsync("package id"))
                .Returns(Task.FromResult(new Package
                    { Name = "package", DeployableLocation = "location", ExecutableFile = "file", Id = "package id" }));
            A.CallTo(() => dbContext.HostsRepository.UpdateAsync(A<Host>.Ignored))
                .Invokes((Host host) =>
                {
                    Assert.Equal(43, host.NextFreePort);
                })
                .Returns(Task.FromResult(new Host()));
            A.CallTo(() => dbContext.ApplicationDeploysRepository.SaveAsync(A<ApplicationDeploy>.Ignored))
                .Invokes((ApplicationDeploy deploy) =>
                {
                    Assert.Equal("application id", deploy.ApplicationId);
                    Assert.Equal("host id", deploy.HostId);
                    Assert.False(deploy.IsCompleted);
                    Assert.False(deploy.IsSuccessful);
                })
                .Returns(Task.FromResult(new ApplicationDeploy()));

            var ssh = A.Fake<ICoreSsh>();

            var deployFolder = "/home/ssh user name/application";

            A.CallTo(() => ssh.ConnectAsync("address", "ssh user name", "key file"))
                .Returns(Task.FromResult(SshResult.Success()));
            A.CallTo(() => ssh.UploadDirectoryAsync("location", deployFolder))
                .Returns(Task.FromResult(SshResult.Success()));
            A.CallTo(() => ssh.RunCommandAsync($"chmod +x {deployFolder}/file"))
                .Returns(Task.FromResult(SshResult.Success()));
            A.CallTo(() => ssh.UploadContentAsync(A<string>.Ignored, $"{deployFolder}/application.service"))
                .Invokes((string content, string location) =>
                {
                    var serviceContents =
                        $"[Unit]\r\nDescription=Application application, package package\r\nAfter=network.target\r\n\r\n[Service]\r\nWorkingDirectory={deployFolder}\r\nExecStart={deployFolder}/file\r\nRestart=always\r\n# User and group under which the service runs\r\nUser=ssh user name\r\nGroup=ssh user name\r\nEnvironment=DOTNET_ENVIRONMENT=Production\r\nEnvironment=ASPNETCORE_URLS=http://0.0.0.0:42\r\n\r\n[Install]\r\nWantedBy=multi-user.target";

                    Assert.Equal(serviceContents, content);
                    Assert.Equal("/home/ssh user name/application/application.service", location);
                })
                .Returns(Task.FromResult(SshResult.Success()));
            A.CallTo(() => ssh.RunCommandAsync($"sudo mv {deployFolder}/application.service /etc/systemd/system/application.service"))
                .Returns(Task.FromResult(SshResult.Success()));
            A.CallTo(() => ssh.RunCommandAsync("sudo systemctl daemon-reload"))
                .Returns(Task.FromResult(SshResult.Success()));
            A.CallTo(() => ssh.RunCommandAsync("sudo systemctl enable application.service"))
                .Returns(Task.FromResult(SshResult.Success()));
            A.CallTo(() => ssh.RunCommandAsync("sudo systemctl start application.service"))
                .Returns(Task.FromResult(SshResult.Success()));
            A.CallTo(() => ssh.RunCommandAsync("sudo systemctl status application.service"))
                .Returns(Task.FromResult(SshResult.Success()));

            A.CallTo(() => dbContext.ActiveDeploymentsRepository.CleanupDeploymentsOfApplicationAsync("application id"))
                .Returns(Task.CompletedTask);
            A.CallTo(() => dbContext.ActiveDeploymentsRepository.SaveAsync(A<ActiveDeployment>.Ignored))
                .Invokes((ActiveDeployment deploy) =>
                {
                    Assert.Equal("package id", deploy.PackageId);
                    Assert.Equal("application id", deploy.ApplicationId);
                    Assert.Equal(42, deploy.Port);
                })
                .Returns(Task.FromResult(new ActiveDeployment()));

            A.CallTo(() => dbContext.JournalEntriesRepository.SaveAsync(A<JournalEntry>.Ignored))
                .Returns(Task.FromResult(new JournalEntry()));

            var elbClient = A.Fake<IAmazonElasticLoadBalancingV2>(x => x.Strict());
            A.CallTo(() =>
                    elbClient.CreateTargetGroupAsync(A<CreateTargetGroupRequest>.Ignored, A<CancellationToken>.Ignored))
                .Invokes((CreateTargetGroupRequest request, CancellationToken _) =>
                    {
                        Assert.Equal("application-target-group", request.Name);
                        Assert.Equal(ProtocolEnum.HTTP.ToString(), request.Protocol);
                        Assert.Equal(42, request.Port);
                        Assert.Equal("vpc id", request.VpcId);
                        Assert.Equal(TargetTypeEnum.Instance.ToString(), request.TargetType);
                        Assert.True(request.HealthCheckEnabled);
                    })
                .Returns(Task.FromResult(new CreateTargetGroupResponse
                {
                    TargetGroups =
                    [
                        new()
                        {
                            TargetGroupArn = "target group arn"
                        }
                    ]
                }));
            A.CallTo(() =>
                    elbClient.RegisterTargetsAsync(A<RegisterTargetsRequest>.Ignored, A<CancellationToken>.Ignored))
                .Invokes((RegisterTargetsRequest request, CancellationToken _) =>
                {
                    Assert.Equal("target group arn", request.TargetGroupArn);
                })
                .Returns(Task.FromResult(new RegisterTargetsResponse()));
            A.CallTo(() =>
                    elbClient.DescribeListenersAsync(A<DescribeListenersRequest>.Ignored, A<CancellationToken>.Ignored))
                .Invokes((DescribeListenersRequest request, CancellationToken _) =>
                {
                    Assert.Equal("load balancer arn", request.LoadBalancerArn);
                })
                .Returns(Task.FromResult(new DescribeListenersResponse
                {
                    Listeners =
                    [
                        new()
                        {
                            Port = 80,
                            ListenerArn = "listener arn"
                        }
                    ]
                }));
            A.CallTo(() => elbClient.CreateRuleAsync(A<CreateRuleRequest>.Ignored, A<CancellationToken>.Ignored))
                .Invokes((CreateRuleRequest request, CancellationToken _) =>
                {
                    Assert.Single(request.Tags);
                    Assert.Equal("Name", request.Tags[0].Key);
                    Assert.Equal("application-rule", request.Tags[0].Value);
                    Assert.Equal("listener arn", request.ListenerArn);
                    Assert.Single(request.Actions);
                    Assert.Equal("target group arn", request.Actions[0].TargetGroupArn);
                    Assert.Equal(ActionTypeEnum.Forward.ToString(), request.Actions[0].Type);
                    Assert.Single(request.Conditions);
                    Assert.Equal("host-header", request.Conditions[0].Field);
                    Assert.Equal(2, request.Conditions[0].HostHeaderConfig.Values.Count);
                    Assert.Contains("www.domain.com", request.Conditions[0].HostHeaderConfig.Values);
                    Assert.Contains("*.www.domain.com", request.Conditions[0].HostHeaderConfig.Values);
                })
                .Returns(Task.FromResult(new CreateRuleResponse()));

            A.CallTo(() => ssh.DisconnectAsync())
                .DoesNothing();

            A.CallTo(() => dbContext.ApplicationDeploysRepository.UpdateAsync(A<ApplicationDeploy>.Ignored))
                .Invokes((ApplicationDeploy deploy) =>
                {
                    Assert.True(deploy.IsCompleted);
                    Assert.True(deploy.IsSuccessful);
                })
                .Returns(Task.FromResult(new ApplicationDeploy()));

            var service = new DeployOrchestrator(dbContext, ssh, elbClient);

            await service.DeployAsync(new Application
            {
                Id = "application id",
                Name = "application",
                Domain = "www.domain.com",
                PackageId = "package id"
            }, new Host
            {
                Id = "host id",
                Address = "address",
                AssignedLoadBalancerArn = "load balancer arn",
                InstanceId = "instance id",
                KeyFile = "key file",
                NextFreePort = 42,
                SshUserName = "ssh user name",
                VpcId = "vpc id"
            });

            A.CallTo(() => dbContext.PackagesRepository.GetByIdAsync("package id"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => dbContext.HostsRepository.UpdateAsync(A<Host>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => dbContext.ApplicationDeploysRepository.SaveAsync(A<ApplicationDeploy>.Ignored))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => ssh.ConnectAsync("address", "ssh user name", "key file"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => ssh.UploadDirectoryAsync("location", deployFolder))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => ssh.RunCommandAsync($"chmod +x {deployFolder}/file"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => ssh.UploadContentAsync(A<string>.Ignored, $"{deployFolder}/application.service"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => ssh.RunCommandAsync($"sudo mv {deployFolder}/application.service /etc/systemd/system/application.service"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => ssh.RunCommandAsync("sudo systemctl daemon-reload"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => ssh.RunCommandAsync("sudo systemctl enable application.service"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => ssh.RunCommandAsync("sudo systemctl start application.service"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => ssh.RunCommandAsync("sudo systemctl status application.service"))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => dbContext.ActiveDeploymentsRepository.CleanupDeploymentsOfApplicationAsync("application id"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => dbContext.ActiveDeploymentsRepository.SaveAsync(A<ActiveDeployment>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => dbContext.JournalEntriesRepository.SaveAsync(A<JournalEntry>.Ignored))
                .MustHaveHappened(4, Times.Exactly);

            A.CallTo(() =>
                    elbClient.CreateTargetGroupAsync(A<CreateTargetGroupRequest>.Ignored, A<CancellationToken>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() =>
                    elbClient.RegisterTargetsAsync(A<RegisterTargetsRequest>.Ignored, A<CancellationToken>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() =>
                    elbClient.DescribeListenersAsync(A<DescribeListenersRequest>.Ignored, A<CancellationToken>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => elbClient.CreateRuleAsync(A<CreateRuleRequest>.Ignored, A<CancellationToken>.Ignored))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => ssh.DisconnectAsync())
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => dbContext.ApplicationDeploysRepository.UpdateAsync(A<ApplicationDeploy>.Ignored))
                .MustHaveHappenedOnceExactly();
        }
    }
}
