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
        private readonly ICrudRepository<T> _repo;

        public CrudService(IDbContext dbContext) : this()
        {
            if (typeof(T) == typeof(Host))
            {
                _repo = (ICrudRepository<T>)dbContext.HostsRepository;
            }
            else if (typeof(T) == typeof(Package))
            {
                _repo = (ICrudRepository<T>)dbContext.PackagesRepository;
            }
        }

        public async Task<T> SaveAsync(T entity)
        {
            return await _repo.SaveAsync(entity);
        }

        public async Task<T> UpdateAsync(T entity)
        {
            return await _repo.UpdateAsync(entity);
        }

        public async Task DeleteAsync(T entity)
        {
            await _repo.DeleteAsync(entity);
        }

        public async Task<T> GetByIdAsync(string id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<IList<T>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }
    }
}
