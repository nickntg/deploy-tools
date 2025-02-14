using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.DataAccess.Repositories.Interfaces;
using NHibernate;
using NHibernate.Criterion;

namespace DeployTools.Core.DataAccess.Repositories
{
    public class CertificatesRepository(ISession session) : CrudRepository<Certificate>(session), ICertificatesRepository
    {
        public async Task<IList<Certificate>> GetCertificatesMarkedForDeletionAsync()
        {
            return await Session.CreateCriteria<Certificate>()
                .Add(Restrictions.Eq(nameof(Certificate.IsMarkedForDeletion), true))
                .ListAsync<Certificate>();
        }

        public async Task<Certificate> GetCertificateByDomainAsync(string domain)
        {
            return await Session.CreateCriteria<Certificate>()
                .Add(Restrictions.Eq(nameof(Certificate.Domain), domain))
                .UniqueResultAsync<Certificate>();
        }

        public async Task<IList<Certificate>> GetUnvalidatedCertificatesAsync()
        {
            return await Session.CreateCriteria<Certificate>()
                .Add(Restrictions.Eq(nameof(Certificate.IsValidated), false))
                .ListAsync<Certificate>();
        }

        public async Task<IList<Certificate>> GetCertificatesAboutToExpireAsync(int daysUntilExpiration)
        {
            return await Session.CreateCriteria<Certificate>()
                .Add(Restrictions.Le(nameof(Certificate.ExpiresAt), DateTimeOffset.UtcNow.AddDays(daysUntilExpiration)))
                .ListAsync<Certificate>();
        }

        public async Task<IList<Certificate>> GetCertificatesNotCreatedAsync()
        {
            return await Session.CreateCriteria<Certificate>()
                .Add(Restrictions.Eq(nameof(Certificate.IsCreated), false))
                .ListAsync<Certificate>();
        }
    }
}
