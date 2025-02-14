using System;

namespace DeployTools.Core.Models
{
    public class CertificateDescribeResult
    {
        public bool HasBeenValidated { get; set; }
        public DateTimeOffset? Expiry { get; set; }
    }
}
