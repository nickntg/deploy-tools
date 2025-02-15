using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.CertificateManager;
using Amazon.CertificateManager.Model;
using DeployTools.Core.DataAccess.Context.Interfaces;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Models;
using DeployTools.Core.Models.Background;
using DeployTools.Core.Services.Background;
using DeployTools.Core.Services.Interfaces;

namespace DeployTools.Core.Services
{
    internal class CertificatesService(IDbContext dbContext, IAmazonCertificateManager certificateManager) : CrudService<Certificate>(dbContext), ICertificatesService
    {
        private readonly IDbContext _dbContext = dbContext;

        public async Task<IList<Certificate>> GetCertificatesMarkedForDeletionAsync()
        {
            return await _dbContext.CertificateRepository.GetCertificatesMarkedForDeletionAsync();
        }

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

        public override async Task<Certificate> SaveAsync(Certificate entity)
        {
            var dbEntity = await base.SaveAsync(entity);

            await SaveJob(new CertificateJobInfo
            {
                CertificateAction = CertificateAction.Create,
                CertificateId = dbEntity.Id
            }, nameof(CertificateAdminJob));

            return dbEntity;
        }

        public override async Task<Certificate> UpdateAsync(Certificate entity)
        {
            var dbEntity = await base.UpdateAsync(entity);

            if (dbEntity.IsMarkedForDeletion)
            {
                await SaveJob(new CertificateJobInfo
                {
                    CertificateAction = CertificateAction.Delete,
                    CertificateId = dbEntity.Id
                }, nameof(CertificateAdminJob));
            }

            return dbEntity;
        }

        public async Task<CertificateDescribeResult> IsCertificatePendingValidationAsync(Certificate certificate)
        {
            var describeResponse = await certificateManager.DescribeCertificateAsync(new DescribeCertificateRequest
            {
                CertificateArn = certificate.Arn
            });

            var response = new CertificateDescribeResult
            {
                HasBeenValidated = describeResponse.Certificate.Status != CertificateStatus.PENDING_VALIDATION
            };

            if (response.HasBeenValidated)
            {
                response.Expiry = new DateTimeOffset(describeResponse.Certificate.NotAfter);
            }

            return response;
        }

        public async Task DeleteCertificateFromAcmAsync(Certificate certificate)
        {
            var request = new DeleteCertificateRequest
            {
                CertificateArn = certificate.Arn
            };

            await certificateManager.DeleteCertificateAsync(request);
        }

        public async Task<CertificateCreationResult> CreateCertificateAsync(Certificate certificate)
        {
            var certificateRequest = new RequestCertificateRequest
            {
                DomainName = certificate.Domain,
                SubjectAlternativeNames = [$"*.{certificate.Domain}"],
                ValidationMethod = ValidationMethod.DNS
            };

            var response = await certificateManager.RequestCertificateAsync(certificateRequest);

            var arn = response.CertificateArn;

            DescribeCertificateResponse describeResponse = null;

            var attempts = 0;

            const int maxAttempts = 10;
            const int delaySeconds = 5;

            var recordsAvailable = false;

            while (attempts < maxAttempts && !recordsAvailable)
            {
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));

                describeResponse = await certificateManager.DescribeCertificateAsync(new DescribeCertificateRequest
                {
                    CertificateArn = arn
                });

                recordsAvailable = describeResponse.Certificate.DomainValidationOptions
                    .All(option => option.ResourceRecord != null);
                attempts++;
            }

            var validationDetails = describeResponse?.Certificate.DomainValidationOptions
                .Select(option => new DomainValidationDetails
                {
                    DomainName = option.DomainName,
                    ResourceRecordName = option.ResourceRecord?.Name,
                    ResourceRecordType = option.ResourceRecord?.Type,
                    ResourceRecordValue = option.ResourceRecord?.Value
                })
                .ToList();

            return new CertificateCreationResult
            {
                CertificateArn = arn,
                CertificateId = arn.Split('/').Last(),
                DomainValidationOptions = validationDetails
            };
        }

        private async Task SaveJob(object jobInfo, string jobType)
        {
            var job = new Job
            {
                IsProcessed = false,
                Type = jobType,
                SerializedInfo = JsonSerializer.Serialize(jobInfo),
            };

            await _dbContext.JobsRepository.SaveAsync(job);
        }
    }
}
