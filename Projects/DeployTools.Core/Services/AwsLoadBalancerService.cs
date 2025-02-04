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
            Log.Info($"Removing target group {targetGroupArn}");

            var @event = new JournalEventArgs
            {
                CommandExecuted = $"Removing target group {targetGroupArn}",
                WasSuccessful = true
            };

            try
            {
                return await elbClient.DeleteTargetGroupAsync(new DeleteTargetGroupRequest
                {
                    TargetGroupArn = targetGroupArn
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex);

                @event.WasSuccessful = false;

                throw;
            }
            finally
            {
                logFunction(@event);
            }
        }

        public static async Task<DeleteRuleResponse> DeleteRuleAsync(IAmazonElasticLoadBalancingV2 elbClient,
            string ruleArn,
            Action<JournalEventArgs> logFunction)
        {
            Log.Info($"Removing rule {ruleArn}");

            var @event = new JournalEventArgs
            {
                CommandExecuted = $"Removing rule {ruleArn}",
                WasSuccessful = true
            };

            try
            {
                return await elbClient.DeleteRuleAsync(new DeleteRuleRequest
                {
                    RuleArn = ruleArn
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex);

                @event.WasSuccessful = false;

                throw;
            }
            finally
            {
                logFunction(@event);
            }
        }

        public static async Task<DescribeTargetGroupsResponse> DescribeTargetGroupsAsync(
            IAmazonElasticLoadBalancingV2 elbClient,
            string loadBalancerArn,
            Action<JournalEventArgs> logFunction)
        {
            var message = string.IsNullOrEmpty(loadBalancerArn)
                ? $"Retrieving target groups of load balancer {loadBalancerArn}"
                : "Retrieving all target groups";

            Log.Info(message);

            var @event = new JournalEventArgs
            {
                CommandExecuted = message,
                WasSuccessful = true
            };

            try
            {
                var request = new DescribeTargetGroupsRequest();

                if (!string.IsNullOrEmpty(loadBalancerArn))
                {
                    request.LoadBalancerArn = loadBalancerArn;
                }

                return await elbClient.DescribeTargetGroupsAsync(request);
            }
            catch (Exception ex)
            {
                Log.Error(ex);

                @event.WasSuccessful = false;

                throw;
            }
            finally
            {
                logFunction(@event);
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
            Log.Info($"Adding rule to listener {listenerArn} - domain {domain}");

            var @event = new JournalEventArgs
            {
                CommandExecuted = $"Add rule to listener for domain {domain}",
                WasSuccessful = true
            };

            try
            {
                return await elbClient.CreateRuleAsync(new CreateRuleRequest
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
            catch (Exception ex)
            {
                Log.Error(ex);

                @event.WasSuccessful = false;

                throw;
            }
            finally
            {
                logFunction(@event);
            }
        }

        public static async Task<DescribeRulesResponse> DescribeRulesAsync(IAmazonElasticLoadBalancingV2 elbClient,
            string listenerArn,
            Action<JournalEventArgs> logFunction)
        {
            var @event = new JournalEventArgs
            {
                CommandExecuted = $"List rules for listener {listenerArn}",
                WasSuccessful = true
            };

            Log.Info($"List rules for listener {listenerArn}");

            try
            {
                return await elbClient.DescribeRulesAsync(new DescribeRulesRequest
                {
                    ListenerArn = listenerArn
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex);

                @event.WasSuccessful = false;

                throw;
            }
            finally
            {
                logFunction(@event);
            }
        }

        public static async Task<DescribeListenersResponse> DescribeListenersAsync(
            IAmazonElasticLoadBalancingV2 elbClient,
            string assignedLoadBalancerArn,
            Action<JournalEventArgs> logFunction)
        {
            Log.Info("Retrieve listeners");

            var @event = new JournalEventArgs
            {
                CommandExecuted = "Retrieve listeners",
                WasSuccessful = true
            };

            try
            {
                return await elbClient.DescribeListenersAsync(new DescribeListenersRequest()
                {
                    LoadBalancerArn = assignedLoadBalancerArn
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex);

                @event.WasSuccessful = false;

                throw;
            }
            finally
            {
                logFunction(@event);
            }
        }

        public static async Task<RegisterTargetsResponse> RegisterTargetsAsync(IAmazonElasticLoadBalancingV2 elbClient,
            string targetGroupName,
            string targetGroupArn,
            string instanceId,
            Action<JournalEventArgs> logFunction)
        {
            Log.Info($"Registering instance {instanceId} to target group {targetGroupName}");

            var @event = new JournalEventArgs
            {
                CommandExecuted = $"Register instance {instanceId} to target group {targetGroupName}",
                WasSuccessful = true
            };

            try
            {
                return await elbClient.RegisterTargetsAsync(new RegisterTargetsRequest
                {
                    TargetGroupArn = targetGroupArn,
                    Targets = [new() { Id = instanceId }]
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex);

                @event.WasSuccessful = false;

                throw;
            }
            finally
            {
                logFunction(@event);
            }
        }

        public static async Task<CreateTargetGroupResponse> CreateTargetGroupAsync(IAmazonElasticLoadBalancingV2 elbClient,
            string targetGroupName,
            int deployPort,
            string vpcId,
            Action<JournalEventArgs> logFunction)
        {
            Log.Info($"Creating target group {targetGroupName}");

            var @event = new JournalEventArgs
            {
                CommandExecuted = $"Create target group {targetGroupName}",
                WasSuccessful = true
            };

            try
            {
                return await elbClient.CreateTargetGroupAsync(new CreateTargetGroupRequest
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
            catch (Exception)
            {
                @event.WasSuccessful = false;

                throw;
            }
            finally
            {
                logFunction(@event);
            }
        }

        //private static async Task<AmazonWebServiceResponse> ExecuteCommandAsync(string message, 
        //    Func<Task<AmazonWebServiceResponse>> callFunc,
        //    Action<JournalEventArgs> logFunction)
        //{
        //    Log.Info(message);

        //    var @event = new JournalEventArgs
        //    {
        //        CommandExecuted = message,
        //        WasSuccessful = true
        //    };

        //    try
        //    {
        //        return await callFunc();
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex);

        //        @event.WasSuccessful = false;

        //        throw;
        //    }
        //    finally
        //    {
        //        logFunction(@event);
        //    }
        //}
    }
}
