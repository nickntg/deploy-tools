using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.RDS;
using Amazon.RDS.Model;
using DeployTools.Core.DataAccess.Context.Interfaces;
using DeployTools.Core.DataAccess.Entities;

using DeployTools.Core.Services.Interfaces;

namespace DeployTools.Core.Services
{
    public class RdsEnginesService(IDbContext dbContext, IAmazonRDS rds) : IRdsEnginesService
    {
        public async Task RefreshAsync()
        {
            await dbContext.RdsEnginesRepository.ClearAsync();

            var continuation = string.Empty;

            do
            {
                var request = new DescribeDBEngineVersionsRequest
                {
                    Marker = continuation
                };
                var response = await rds.DescribeDBEngineVersionsAsync(request);

                continuation = response.Marker;

                foreach (var o in response.DBEngineVersions)
                {
                    await dbContext.RdsEnginesRepository.SaveAsync(new AwsRdsEngine
                    {
                        EngineName = o.Engine,
                        EngineVersion = o.EngineVersion,
                        Status = o.Status
                    });
                }
            } while (!string.IsNullOrEmpty(continuation));
        }

        public async Task<IList<AwsRdsEngine>> GetAllAsync()
        {
            return await dbContext.RdsEnginesRepository.GetAllSortedAsync();
        }
    }
}
