using System;
using System.Text.Json;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Models;
using DeployTools.Core.Models.Background;
using DeployTools.Core.Services.Background;
using DeployTools.Core.Services.Interfaces;
using DeployTools.Core.Tests.Helpers;
using FakeItEasy;
using Xunit;
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace DeployTools.Core.Tests.Services.Background
{
    public class CertificateAdminJobTests
    {
        [Fact]
        public async Task InvalidCertificateAction()
        {
            var job = new CertificateAdminJob(null, null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => job.ProcessJobAsync(new Job
            {
                SerializedInfo = JsonSerializer.Serialize(new CertificateJobInfo
                    { CertificateAction = (CertificateAction)42 })
            }));
        }

        [Fact]
        public async Task CreateCertificateNotFound()
        {
            await PerformCertificateActionNotFound(CertificateAction.Create);
        }

        [Fact]
        public async Task CreateCertificate()
        {
            var certificateService = A.Fake<ICertificatesService>(x => x.Strict());
            A.CallTo(() => certificateService.GetByIdAsync("certificate id"))
                .Returns(Task.FromResult(new Certificate()));
            A.CallTo(() => certificateService.CreateCertificateAsync(A<Certificate>.Ignored))
                .Returns(Task.FromResult(new CertificateCreationResult
                {
                    CertificateId = "certificate id",
                    CertificateArn = "certificate arn",
                    DomainValidationOptions = [new DomainValidationDetails
                    {
                        DomainName = "domain.com",
                        ResourceRecordName = "name",
                        ResourceRecordType = "CNAME",
                        ResourceRecordValue = "value"
                    },
                    new DomainValidationDetails
                    {
                        DomainName = "domain.com",
                        ResourceRecordName = "name2",
                        ResourceRecordType = "CNAME",
                        ResourceRecordValue = "value2"
                    }]
                }));
            A.CallTo(() => certificateService.UpdateAsync(A<Certificate>.Ignored))
                .Invokes((Certificate certificate) =>
                {
                    Assert.True(certificate.IsCreated);
                    Assert.Equal("certificate id", certificate.CertificateId);
                    Assert.Equal("certificate arn", certificate.Arn);
                    Assert.Equal("domain.com\r\nname\r\nCNAME\r\nvalue\r\ndomain.com\r\nname2\r\nCNAME\r\nvalue2", certificate.ValidationInfo);

                })
                .Returns(Task.FromResult(new Certificate()));

            var dbContext = DbHelpers.New();
            A.CallTo(() => dbContext.JobsRepository.SaveAsync(A<Job>.Ignored))
                .Invokes((Job job) =>
                {
                    Assert.False(job.IsProcessed);
                    Assert.Equal(nameof(CertificateAdminJob), job.Type);
                    
                    var info = JsonSerializer.Deserialize<CertificateJobInfo>(job.SerializedInfo);
                    Assert.Equal(CertificateAction.WaitForValidation, info.CertificateAction);
                    Assert.Equal("certificate id", info.CertificateId);
                })
                .Returns(Task.FromResult(new Job()));

            var job = new CertificateAdminJob(dbContext, certificateService);

            await job.ProcessJobAsync(new Job
            {
                SerializedInfo = JsonSerializer.Serialize(new CertificateJobInfo
                {
                    CertificateAction = CertificateAction.Create,
                    CertificateId = "certificate id"
                })
            });

            A.CallTo(() => certificateService.GetByIdAsync("certificate id"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => certificateService.CreateCertificateAsync(A<Certificate>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => certificateService.UpdateAsync(A<Certificate>.Ignored))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => dbContext.JobsRepository.SaveAsync(A<Job>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task CheckCertificateNotFound()
        {
            await PerformCertificateActionNotFound(CertificateAction.WaitForValidation);
        }

        [Fact]
        public async Task CheckCertificateNotYetValidated()
        {
            var certificateService = A.Fake<ICertificatesService>(x => x.Strict());
            A.CallTo(() => certificateService.GetByIdAsync("certificate id"))
                .Returns(Task.FromResult(new Certificate()));
            A.CallTo(() => certificateService.IsCertificatePendingValidationAsync(A<Certificate>.Ignored))
                .Returns(Task.FromResult(new CertificateDescribeResult
                {
                    HasBeenValidated = false
                }));

            var job = new CertificateAdminJob(null, certificateService);

            await Assert.ThrowsAsync<InvalidOperationException>(() => job.ProcessJobAsync(new Job
            {
                SerializedInfo = JsonSerializer.Serialize(new CertificateJobInfo
                {
                    CertificateAction = CertificateAction.WaitForValidation,
                    CertificateId = "certificate id"
                })
            }));

            A.CallTo(() => certificateService.GetByIdAsync("certificate id"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => certificateService.IsCertificatePendingValidationAsync(A<Certificate>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task CheckCertificateHasBeenValidated()
        {
            var certificateService = A.Fake<ICertificatesService>(x => x.Strict());
            A.CallTo(() => certificateService.GetByIdAsync("certificate id"))
                .Returns(Task.FromResult(new Certificate()));
            var dt = DateTimeOffset.UtcNow;
            A.CallTo(() => certificateService.IsCertificatePendingValidationAsync(A<Certificate>.Ignored))
                .Returns(Task.FromResult(new CertificateDescribeResult
                {
                    Expiry = dt,
                    HasBeenValidated = true
                }));
            A.CallTo(() => certificateService.UpdateAsync(A<Certificate>.Ignored))
                .Invokes((Certificate certificate) =>
                {
                    Assert.True(certificate.IsValidated);
                    Assert.Equal(dt, certificate.ExpiresAt);
                })
                .Returns(Task.FromResult(new Certificate()));

            var job = new CertificateAdminJob(null, certificateService);

            await job.ProcessJobAsync(new Job
            {
                SerializedInfo = JsonSerializer.Serialize(new CertificateJobInfo
                {
                    CertificateAction = CertificateAction.WaitForValidation,
                    CertificateId = "certificate id"
                })
            });

            A.CallTo(() => certificateService.GetByIdAsync("certificate id"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => certificateService.IsCertificatePendingValidationAsync(A<Certificate>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => certificateService.UpdateAsync(A<Certificate>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task RemoveCertificateNotFound()
        {
            await PerformCertificateActionNotFound(CertificateAction.Delete);
        }

        [Fact]
        public async Task RemoveCertificate()
        {
            var certificateService = A.Fake<ICertificatesService>(x => x.Strict());
            A.CallTo(() => certificateService.GetByIdAsync("certificate id"))
                .Returns(Task.FromResult(new Certificate()));
            A.CallTo(() => certificateService.DeleteCertificateFromAcmAsync(A<Certificate>.Ignored))
                .DoesNothing();
            A.CallTo(() => certificateService.DeleteAsync(A<Certificate>.Ignored))
                .Returns(Task.FromResult(new Certificate()));

            var job = new CertificateAdminJob(null, certificateService);

            await job.ProcessJobAsync(new Job
            {
                SerializedInfo = JsonSerializer.Serialize(new CertificateJobInfo
                {
                    CertificateAction = CertificateAction.Delete,
                    CertificateId = "certificate id"
                })
            });

            A.CallTo(() => certificateService.GetByIdAsync("certificate id"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => certificateService.DeleteCertificateFromAcmAsync(A<Certificate>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => certificateService.DeleteAsync(A<Certificate>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        private async Task PerformCertificateActionNotFound(CertificateAction action)
        {
            var certificateService = A.Fake<ICertificatesService>(x => x.Strict());
            A.CallTo(() => certificateService.GetByIdAsync("certificate id"))
                .Returns(Task.FromResult<Certificate>(null));

            var job = new CertificateAdminJob(null, certificateService);

            await job.ProcessJobAsync(new Job
            {
                SerializedInfo = JsonSerializer.Serialize(new CertificateJobInfo
                {
                    CertificateAction = action,
                    CertificateId = "certificate id"
                })
            });

            A.CallTo(() => certificateService.GetByIdAsync("certificate id"))
                .MustHaveHappenedOnceExactly();
        }
    }
}