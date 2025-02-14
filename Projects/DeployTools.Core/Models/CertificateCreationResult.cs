using System.Collections.Generic;

namespace DeployTools.Core.Models
{
    public class CertificateCreationResult
    {
        public string CertificateArn { get; set; }
        public string CertificateId { get; set; }
        public List<DomainValidationDetails> DomainValidationOptions { get; set; }
    }
}
