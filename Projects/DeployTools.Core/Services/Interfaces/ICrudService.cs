using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.Services.Interfaces
{
    public interface ICrudService<T> where T: BaseEntity
    {
        Task<T> SaveAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<T> GetByIdAsync(string id);
        Task<IList<T>> GetAllAsync();
    }
}