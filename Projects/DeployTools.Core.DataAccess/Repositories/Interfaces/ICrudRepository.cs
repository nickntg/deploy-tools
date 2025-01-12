using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeployTools.Core.DataAccess.Repositories.Interfaces
{
    public interface ICrudRepository<T>
    {
        Task<T> SaveAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<T> GetByIdAsync(string id);
        Task<IList<T>> GetAllAsync();
    }
}
