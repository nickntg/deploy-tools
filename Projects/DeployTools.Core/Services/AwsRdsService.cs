using System;
using System.Threading.Tasks;
using Amazon.RDS;
using Amazon.RDS.Model;
using DeployTools.Core.Helpers;

namespace DeployTools.Core.Services
{
    public class AwsRdsService
    {
        public static async Task<CreateDBInstanceResponse> CreateDbInstanceAsync(IAmazonRDS rdsClient,
            string dbInstanceIdentifier,
            string engine,
            string engineVersion,
            string dbUserName,
            string dbPassword,
            string dbInstance,
            string vpcSecurityId,
            string dbSubnetGroupName,
            string dbName,
            string storageType,
            int? storageGigabytes,
            Action<JournalEventArgs> logFunction)
        {
            var response = await AwsCommandExecutor.ExecuteCommandAsync("Create DB instance", Operation, logFunction);

            logFunction(new JournalEventArgs
            {
                CommandCompleted = DateTimeOffset.UtcNow,
                CommandExecuted = $"RDS instance {response.DBInstance.DBInstanceArn} created"
            });

            return response;

            Task<CreateDBInstanceResponse> Operation()
            {
                var createRequest = new CreateDBInstanceRequest
                {
                    DBInstanceIdentifier = dbInstanceIdentifier,
                    Engine = engine,
                    EngineVersion = engineVersion,
                    MasterUsername = dbUserName,
                    MasterUserPassword = dbPassword,
                    DBInstanceClass = dbInstance,
                    VpcSecurityGroupIds = [vpcSecurityId],
                    PubliclyAccessible = false,
                    MultiAZ = false,
                    BackupRetentionPeriod = 7,
                    StorageEncrypted = true,
                    DeletionProtection = false,
                    DBSubnetGroupName = dbSubnetGroupName,
                    DBName = dbName,
                    MonitoringInterval = 0
                };

                if (!string.IsNullOrEmpty(storageType))
                {
                    createRequest.StorageType = storageType;
                    createRequest.AllocatedStorage = storageGigabytes ?? 20;
                    createRequest.MaxAllocatedStorage = storageGigabytes ?? 20;
                }

                return rdsClient.CreateDBInstanceAsync(createRequest);
            }
        }

        public static async Task<DescribeDBInstancesResponse> DescribeDbInstanceAsync(IAmazonRDS rdsClient,
            string dbArn, Action<JournalEventArgs> logFunction)
        {
            return await AwsCommandExecutor.ExecuteCommandAsync($"Getting info on RDS {dbArn}", Operation, logFunction);

            Task<DescribeDBInstancesResponse> Operation() => rdsClient.DescribeDBInstancesAsync(new DescribeDBInstancesRequest
            {
                DBInstanceIdentifier = dbArn
            });
        }

        public static async Task<DeleteDBInstanceResponse> DeleteDbInstanceAsync(IAmazonRDS rdsClient,
            string dbInstanceIdentifier, Action<JournalEventArgs> logFunction)
        {
            return await AwsCommandExecutor.ExecuteCommandAsync($"Deleting RDS {dbInstanceIdentifier}", Operation, logFunction);

            Task <DeleteDBInstanceResponse> Operation() => rdsClient.DeleteDBInstanceAsync(new DeleteDBInstanceRequest
            {
                DBInstanceIdentifier = dbInstanceIdentifier
            });
        }
    }
}
