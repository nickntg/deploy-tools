using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.Services.Interfaces
{
    public interface IRdsEnginesService
    {
        Task RefreshAsync();
        Task<IList<AwsRdsEngine>> GetAllAsync();
    }
}
