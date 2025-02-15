using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Models;

namespace DeployTools.Core.Services.Interfaces
{
    public interface ICertificatesService : ICrudService<Certificate>
    {
        Task<Certificate> GetCertificateByDomainAsync(string domain);
        Task<IList<Certificate>> GetUnvalidatedCertificatesAsync();
        Task<IList<Certificate>> GetCertificatesAboutToExpireAsync(int daysUntilExpiration);
        Task<IList<Certificate>> GetCertificatesNotCreatedAsync();
        Task<IList<Certificate>> GetCertificatesMarkedForDeletionAsync();
        Task<CertificateCreationResult> CreateCertificateAsync(Certificate certificate);
        Task<CertificateDescribeResult> IsCertificatePendingValidationAsync(Certificate certificate);
        Task DeleteCertificateFromAcmAsync(Certificate certificate);
    }
}
