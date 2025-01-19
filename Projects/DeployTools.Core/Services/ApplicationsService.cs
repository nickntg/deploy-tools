using DeployTools.Core.DataAccess.Context.Interfaces;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeployTools.Core.Services
{
    public class ApplicationsService(IDbContext dbContext): CrudService<Application>(dbContext), IApplicationsService
    {
        private readonly IDbContext _dbContext = dbContext;

        public async Task<IList<Application>> GetApplicationsByNameAsync(string name)
        {
            return await _dbContext.ApplicationsRepository.GetApplicationsByNameAsync(name);
        }

        public async Task<IList<Application>> GetApplicationsByDomainAsync(string domain)
        {
            return await _dbContext.ApplicationsRepository.GetApplicationsByDomainAsync(domain);
        }
    }
}
