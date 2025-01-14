using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.DataAccess.Repositories.Interfaces;
using NHibernate;

namespace DeployTools.Core.DataAccess.Repositories
{
	public class CrudRepository<T>(ISession session) : Repository(session), ICrudRepository<T> where T : BaseEntity
	{
        private readonly ISession _session = session;

        public async Task<T> SaveAsync(T entity)
		{
			entity.CreatedAt = DateTimeOffset.UtcNow;
			await _session.SaveAsync(entity);
			await _session.FlushAsync();
			return entity;
		}

		public async Task<T> UpdateAsync(T entity)
		{
			entity.UpdatedAt = DateTimeOffset.UtcNow;
			await _session.UpdateAsync(entity);
			await _session.FlushAsync();
			return entity;
		}

		public async Task DeleteAsync(T entity)
		{
			await _session.DeleteAsync(entity);
			await _session.FlushAsync();
		}

		public async Task<T> GetByIdAsync(string id)
		{
			return await _session.GetAsync<T>(id);
		}

		public async Task<IList<T>> GetAllAsync()
		{
			return await _session.CreateCriteria<T>()
				.ListAsync<T>();
		}
	}
}