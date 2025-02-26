﻿using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.DataAccess.Repositories.Interfaces;
using NHibernate;
using System.Collections.Generic;
using System.Threading.Tasks;
using NHibernate.Criterion;

namespace DeployTools.Core.DataAccess.Repositories
{
    public class HostsRepository(ISession session) : CrudRepository<Host>(session), IHostsRepository
    {
        public async Task<IList<Host>> GetHostsByInstanceIdAsync(string instanceId)
        {
            return await Session.CreateCriteria<Host>()
                .Add(Restrictions.Eq(nameof(Host.InstanceId), instanceId))
                .ListAsync<Host>();
        }

        public async Task<IList<Host>> GetHostsByAddressAsync(string address)
        {
            return await Session.CreateCriteria<Host>()
                .Add(Restrictions.Eq(nameof(Host.Address), address))
                .ListAsync<Host>();
        }
    }
}
