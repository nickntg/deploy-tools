using System;
using DeployTools.Core.DataAccess.Context.Interfaces;
using DeployTools.Core.Services.Background.Interfaces;
using DeployTools.Core.Services.Interfaces;
using System.Text.Json;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Models.Background;
using NLog;

namespace DeployTools.Core.Services.Background
{
    public class CertificateAdminJob(IDbContext dbContext,
        ICertificatesService certificatesService) : LockableJob(dbContext, nameof(CertificateAdminJob), 10), ICertificateAdminJob
    {
        private readonly IDbContext _dbContext = dbContext;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public override async Task ProcessJobAsync(Job job)
        {
            var jobInfo = JsonSerializer.Deserialize<CertificateJobInfo>(job.SerializedInfo);

            switch (jobInfo.CertificateAction)
            {
                case CertificateAction.Create:
                    await CreateCertificate(jobInfo.CertificateId);
                    break;
                case CertificateAction.WaitForValidation:
                    await CheckIfCertificateHasBeenValidated(jobInfo.CertificateId);
                    break;
                case CertificateAction.Delete:
                    await RemoveCertificatesMarkedForDeletion(jobInfo.CertificateId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task CreateCertificate(string certificateId)
        {
            var certificate = await certificatesService.GetByIdAsync(certificateId);
            if (certificate is null)
            {
                return;
            }

            try
            {
                var response = await certificatesService.CreateCertificateAsync(certificate);

                var validationInfo = string.Empty;
                foreach (var vd in response.DomainValidationOptions)
                {
                    validationInfo =
                        $"{validationInfo}\r\n{vd.DomainName}\r\n{vd.ResourceRecordName}\r\n{vd.ResourceRecordType}\r\n{vd.ResourceRecordValue}";
                }

                certificate.Arn = response.CertificateArn;
                certificate.CertificateId = response.CertificateId;
                certificate.IsCreated = true;
                certificate.ValidationInfo = validationInfo;

                await certificatesService.UpdateAsync(certificate);

                var job = new Job
                {
                    IsProcessed = false,
                    SerializedInfo = JsonSerializer.Serialize(new CertificateJobInfo
                    {
                        CertificateAction = CertificateAction.WaitForValidation,
                        CertificateId = certificateId
                    }),
                    Type = nameof(CertificateAdminJob)
                };

                await _dbContext.JobsRepository.SaveAsync(job);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"While trying to create certificate for domain {certificate.Domain}");
                throw;
            }
        }

        private async Task CheckIfCertificateHasBeenValidated(string certificateId)
        {
            var certificate = await certificatesService.GetByIdAsync(certificateId);
            if (certificate is null)
            {
                return;
            }

            var validationResponse = await certificatesService.IsCertificatePendingValidationAsync(certificate);

            if (validationResponse.HasBeenValidated)
            {
                certificate.IsValidated = true;
                certificate.ExpiresAt = validationResponse.Expiry;

                await certificatesService.UpdateAsync(certificate);

                return;
            }

            throw new InvalidOperationException("Certificate still not validated");
        }

        private async Task RemoveCertificatesMarkedForDeletion(string certificateId)
        {
            var certificate = await certificatesService.GetByIdAsync(certificateId);
            if (certificate is null)
            {
                return;
            }

            await certificatesService.DeleteCertificateFromAcmAsync(certificate);

            await certificatesService.DeleteAsync(certificate);
        }
    }
}
