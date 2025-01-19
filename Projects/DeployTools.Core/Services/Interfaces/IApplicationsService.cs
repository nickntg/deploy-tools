using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.Services.Interfaces
{
    public interface IApplicationsService : ICrudService<Application>
    {
        Task<IList<Application>> GetApplicationsByNameAsync(string name);
        Task<IList<Application>> GetApplicationsByDomainAsync(string domain);
    }
}
