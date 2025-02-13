using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Context.Interfaces;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;

namespace DeployTools.Core.Services
{
    internal class CertificatesService(IDbContext dbContext) : CrudService<Certificate>(dbContext), ICertificatesService
    {
        private readonly IDbContext _dbContext = dbContext;

        public async Task<Certificate> GetCertificateByDomainAsync(string domain)
        {
            return await _dbContext.CertificateRepository.GetCertificateByDomainAsync(domain);
        }

        public async Task<IList<Certificate>> GetUnvalidatedCertificatesAsync()
        {
            return await _dbContext.CertificateRepository.GetUnvalidatedCertificatesAsync();
        }

        public async Task<IList<Certificate>> GetCertificatesAboutToExpireAsync(int daysUntilExpiration)
        {
            return await _dbContext.CertificateRepository.GetCertificatesAboutToExpireAsync(daysUntilExpiration);
        }

        public async Task<IList<Certificate>> GetCertificatesNotCreatedAsync()
        {
            return await _dbContext.CertificateRepository.GetCertificatesNotCreatedAsync();
        }

        public Task CreateCertificateAsync(Certificate certificate)
        {
            throw new NotImplementedException();
        }
    }
}
