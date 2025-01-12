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
		public async Task<T> SaveAsync(T entity)
		{
			entity.CreatedAt = DateTimeOffset.UtcNow;
			await session.SaveAsync(entity);
			await session.FlushAsync();
			return entity;
		}

		public async Task<T> UpdateAsync(T entity)
		{
			entity.UpdatedAt = DateTimeOffset.UtcNow;
			await session.UpdateAsync(entity);
			await session.FlushAsync();
			return entity;
		}

		public async Task DeleteAsync(T entity)
		{
			await session.DeleteAsync(entity);
			await session.FlushAsync();
		}

		public async Task<T> GetByIdAsync(string id)
		{
			return await session.GetAsync<T>(id);
		}

		public async Task<IList<T>> GetAllAsync()
		{
			return await session.CreateCriteria<T>()
				.ListAsync<T>();
		}
	}
}