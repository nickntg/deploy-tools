using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.RDS;
using Amazon.RDS.Model;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NHibernate.Hql.Ast.ANTLR.Tree;

namespace DeployTools.Web.Pages
{
    public class RdsEnginesModel(IRdsEnginesService rdsEnginesService, IRdsPackagesService rdsPackagesService, IAmazonRDS rdsClient) : PageModel
    {
        [BindProperty] public IList<AwsRdsEngine> Engines { get; set; }
        [BindProperty] public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            Engines = await rdsEnginesService.GetAllAsync();

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var rds = await rdsPackagesService.GetByIdAsync("58c4143d69f44c0591b116374bc7f5c5");
            var createRequest = new CreateDBInstanceRequest
            {
                DBInstanceIdentifier = "dbinstance-id",
                Engine = rds.Engine,
                EngineVersion = rds.EngineVersion,
                MasterUsername = "master",
                MasterUserPassword = "123456aABC",
                DBInstanceClass = rds.DbInstance,
                VpcSecurityGroupIds = new List<string> { rds.VpcSecurityGroupId },
                PubliclyAccessible = false,
                MultiAZ = false,
                BackupRetentionPeriod = 7,
                StorageEncrypted = true,
                DeletionProtection = false,
                DBSubnetGroupName = "default-rds-subnet-group",
                DBName = "initialdb",
                MonitoringInterval = 0
            };

            if (!string.IsNullOrEmpty(rds.StorageType))
            {
                createRequest.StorageType = rds.StorageType;
                createRequest.AllocatedStorage = rds.StorageInGigabytes.Value;
                createRequest.MaxAllocatedStorage = rds.StorageInGigabytes.Value;
            }
            //var x = o.DescribeDBInstancesAsync();
                //x.Result.DBInstances[]

            var response = await rdsClient.CreateDBInstanceAsync(createRequest);

            var arn = response.DBInstance.DBInstanceArn;

            while (true)
            {
                var dbInstance = await rdsClient.DescribeDBInstancesAsync(new DescribeDBInstancesRequest
                {
                    DBInstanceIdentifier = arn
                });

                if (dbInstance.DBInstances[0].Endpoint is null)
                {
                    Thread.Sleep(5000);
                }
                else
                {
                    var endpoint = dbInstance.DBInstances[0].Endpoint.Address;
                    var port = dbInstance.DBInstances[0].Endpoint.Port;
                }
            }

            await rdsEnginesService.RefreshAsync();

            return await OnGet();
        }
    }
}
