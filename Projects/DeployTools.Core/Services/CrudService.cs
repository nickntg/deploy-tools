using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Context.Interfaces;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.DataAccess.Repositories.Interfaces;
using DeployTools.Core.Services.Interfaces;

namespace DeployTools.Core.Services
{
    public class CrudService<T>() : ICrudService<T> where T: BaseEntity
    {
        protected readonly ICrudRepository<T> Repo;

        public CrudService(IDbContext dbContext) : this()
        {
            if (typeof(T) == typeof(Host))
            {
                Repo = (ICrudRepository<T>)dbContext.HostsRepository;
            }
            else if (typeof(T) == typeof(Package))
            {
                Repo = (ICrudRepository<T>)dbContext.PackagesRepository;
            }
            else if (typeof(T) == typeof(Application))
            {
                Repo = (ICrudRepository<T>)dbContext.ApplicationsRepository;
            }
            else if (typeof(T) == typeof(RdsPackage))
            {
                Repo = (ICrudRepository<T>)dbContext.RdsPackagesRepository;
            }
            else if (typeof(T) == typeof(Certificate))
            {
                Repo = (ICrudRepository<T>)dbContext.CertificateRepository;
            }
        }

        public async Task<T> SaveAsync(T entity)
        {
            return await Repo.SaveAsync(entity);
        }

        public async Task<T> UpdateAsync(T entity)
        {
            return await Repo.UpdateAsync(entity);
        }

        public async Task DeleteAsync(T entity)
        {
            await Repo.DeleteAsync(entity);
        }

        public async Task<T> GetByIdAsync(string id)
        {
            return await Repo.GetByIdAsync(id);
        }

        public async Task<IList<T>> GetAllAsync()
        {
            return await Repo.GetAllAsync();
        }
    }
}
