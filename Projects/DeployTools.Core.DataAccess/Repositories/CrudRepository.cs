using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.DataAccess.Repositories.Interfaces;
using NHibernate;
using NHibernate.Criterion;

namespace DeployTools.Core.DataAccess.Repositories
{
	public class CrudRepository<T>(ISession session) : Repository(session), ICrudRepository<T> where T : BaseEntity
	{
        public async Task<T> SaveAsync(T entity)
		{
			entity.CreatedAt = DateTimeOffset.UtcNow;
			await Session.SaveAsync(entity);
			await Session.FlushAsync();
			return entity;
		}

		public async Task<T> UpdateAsync(T entity)
		{
			entity.UpdatedAt = DateTimeOffset.UtcNow;
			await Session.UpdateAsync(entity);
			await Session.FlushAsync();
			return entity;
		}

		public async Task DeleteAsync(T entity)
		{
			await Session.DeleteAsync(entity);
			await Session.FlushAsync();
		}

		public async Task<T> GetByIdAsync(string id)
		{
			return await Session.GetAsync<T>(id);
		}

		public async Task<IList<T>> GetAllAsync()
		{
			return await Session.CreateCriteria<T>()
                .AddOrder(new Order(nameof(BaseEntity.CreatedAt), false))
				.ListAsync<T>();
		}
	}
}