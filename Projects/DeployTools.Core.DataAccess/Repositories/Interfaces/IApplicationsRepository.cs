﻿using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.DataAccess.Repositories.Interfaces
{
    public interface IApplicationsRepository : ICrudRepository<Application>
    {
        Task<IList<Application>> GetApplicationsByNameAsync(string name);
        Task<IList<Application>> GetApplicationsByDomainAsync(string domain);
    }
}
