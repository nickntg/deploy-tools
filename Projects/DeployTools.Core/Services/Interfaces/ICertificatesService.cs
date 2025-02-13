using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.Services.Interfaces
{
    public interface ICertificatesService : ICrudService<Certificate>
    {
        Task<Certificate> GetCertificateByDomainAsync(string domain);
        Task<IList<Certificate>> GetUnvalidatedCertificatesAsync();
        Task<IList<Certificate>> GetCertificatesAboutToExpireAsync(int daysUntilExpiration);
        Task<IList<Certificate>> GetCertificatesNotCreatedAsync();
        Task CreateCertificateAsync(Certificate certificate);
    }
}
