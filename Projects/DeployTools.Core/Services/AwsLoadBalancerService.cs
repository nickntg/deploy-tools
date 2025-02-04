using System;
using System.Threading.Tasks;
using Amazon.ElasticLoadBalancingV2;
using Amazon.ElasticLoadBalancingV2.Model;
using NLog;

namespace DeployTools.Core.Services
{
    public class AwsLoadBalancerService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static async Task<DeleteTargetGroupResponse> DeleteTargetGroupAsync(
            IAmazonElasticLoadBalancingV2 elbClient,
            string targetGroupArn,
            Action<JournalEventArgs> logFunction)
        {
            return await ExecuteCommandAsync($"Removing target group {targetGroupArn}", Operation, logFunction);

            Task<DeleteTargetGroupResponse> Operation() => elbClient.DeleteTargetGroupAsync(new DeleteTargetGroupRequest
                {
                    TargetGroupArn = targetGroupArn
                }
            );
        }

        public static async Task<DeleteRuleResponse> DeleteRuleAsync(IAmazonElasticLoadBalancingV2 elbClient,
            string ruleArn,
            Action<JournalEventArgs> logFunction)
        {
            return await ExecuteCommandAsync($"Removing rule {ruleArn}", Operation, logFunction);

            Task<DeleteRuleResponse> Operation() => elbClient.DeleteRuleAsync(new DeleteRuleRequest
            {
                RuleArn = ruleArn
            });
        }

        public static async Task<DescribeTargetGroupsResponse> DescribeTargetGroupsAsync(
            IAmazonElasticLoadBalancingV2 elbClient,
            string loadBalancerArn,
            Action<JournalEventArgs> logFunction)
        {
            return await ExecuteCommandAsync(string.IsNullOrEmpty(loadBalancerArn)
                ? $"Retrieving target groups of load balancer {loadBalancerArn}"
                : "Retrieving all target groups", 
                Operation, 
                logFunction);

            Task<DescribeTargetGroupsResponse> Operation()
            {
                var request = new DescribeTargetGroupsRequest();

                if (!string.IsNullOrEmpty(loadBalancerArn))
                {
                    request.LoadBalancerArn = loadBalancerArn;
                }

                return elbClient.DescribeTargetGroupsAsync(request);
            }
        }
        public static async Task<CreateRuleResponse> CreateRuleAsync(IAmazonElasticLoadBalancingV2 elbClient,
            string ruleName,
            string listenerArn,
            int priority,
            string targetGroupArn,
            string domain,
            Action<JournalEventArgs> logFunction)
        {
            return await ExecuteCommandAsync($"Adding rule to listener {listenerArn} - domain {domain}", Operation, logFunction);
            
            Task<CreateRuleResponse> Operation() => elbClient.CreateRuleAsync(new CreateRuleRequest
            {
                Tags =
                [
                    new()
                    {
                        Key = "Name",
                        Value = ruleName
                    }
                ],
                ListenerArn = listenerArn,
                Priority = priority,
                Actions =
                [
                    new()
                    {
                        TargetGroupArn = targetGroupArn,
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
                            Values = [domain, $"*.{domain}"]
                        }
                    }
                ]
            });
        }

        public static async Task<DescribeRulesResponse> DescribeRulesAsync(IAmazonElasticLoadBalancingV2 elbClient,
            string listenerArn,
            Action<JournalEventArgs> logFunction)
        {
            return await ExecuteCommandAsync($"List rules for listener {listenerArn}", Operation, logFunction);

            Task<DescribeRulesResponse> Operation() => elbClient.DescribeRulesAsync(new DescribeRulesRequest
            {
                ListenerArn = listenerArn
            });
        }

        public static async Task<DescribeListenersResponse> DescribeListenersAsync(
            IAmazonElasticLoadBalancingV2 elbClient,
            string assignedLoadBalancerArn,
            Action<JournalEventArgs> logFunction)
        {
            return await ExecuteCommandAsync("Retrieve listeners", Operation, logFunction);

            Task<DescribeListenersResponse> Operation() => elbClient.DescribeListenersAsync(new DescribeListenersRequest()
            {
                LoadBalancerArn = assignedLoadBalancerArn
            });
        }

        public static async Task<RegisterTargetsResponse> RegisterTargetsAsync(IAmazonElasticLoadBalancingV2 elbClient,
            string targetGroupName,
            string targetGroupArn,
            string instanceId,
            Action<JournalEventArgs> logFunction)
        {
            return await ExecuteCommandAsync($"Registering instance {instanceId} to target group {targetGroupName}", Operation, logFunction);

            Task<RegisterTargetsResponse> Operation() => elbClient.RegisterTargetsAsync(new RegisterTargetsRequest
            {
                TargetGroupArn = targetGroupArn,
                Targets = [new() { Id = instanceId }]
            });
        }

        public static async Task<CreateTargetGroupResponse> CreateTargetGroupAsync(IAmazonElasticLoadBalancingV2 elbClient,
            string targetGroupName,
            int deployPort,
            string vpcId,
            Action<JournalEventArgs> logFunction)
        {
            return await ExecuteCommandAsync($"Creating target group {targetGroupName}", Operation, logFunction);

            Task<CreateTargetGroupResponse> Operation() => elbClient.CreateTargetGroupAsync(new CreateTargetGroupRequest
            {
                Name = targetGroupName,
                Protocol = ProtocolEnum.HTTP,
                Port = deployPort,
                VpcId = vpcId,
                TargetType = TargetTypeEnum.Instance,
                HealthCheckEnabled = true,
                HealthCheckPath = "/",
                HealthCheckIntervalSeconds = 30,
                HealthCheckTimeoutSeconds = 5,
                HealthyThresholdCount = 3,
                UnhealthyThresholdCount = 3
            });
        }

        private static async Task<T> ExecuteCommandAsync<T>(
            string message,
            Func<Task<T>> operation,
            Action<JournalEventArgs> logFunction)
        {
            Log.Info(message);

            var journalEvent = new JournalEventArgs
            {
                CommandExecuted = message,
                WasSuccessful = true
            };

            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                Log.Error(ex);

                journalEvent.WasSuccessful = false;
                
                throw;
            }
            finally
            {
                logFunction(journalEvent);
            }
        }
    }
}
