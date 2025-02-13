using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.DataAccess.Repositories.Interfaces
{
    public interface ICertificatesRepository : ICrudRepository<Certificate>
    {
        Task<Certificate> GetCertificateByDomainAsync(string domain);
        Task<IList<Certificate>> GetUnvalidatedCertificatesAsync();
        Task<IList<Certificate>> GetCertificatesAboutToExpireAsync(int daysUntilExpiration);
        Task<IList<Certificate>> GetCertificatesNotCreatedAsync();
    }
}