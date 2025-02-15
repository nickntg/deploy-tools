using System;

namespace DeployTools.Core.DataAccess.Entities
{
    public class Certificate : BaseEntity
    {
        public virtual string Arn { get; set; }
        public virtual string CertificateId { get; set; }
        public virtual string Domain { get; set; }
        public virtual bool IsValidated { get; set; }
        public virtual bool IsCreated { get; set; }
        public virtual bool IsMarkedForDeletion { get; set; }
        public virtual string ValidationInfo { get; set; }
        public virtual DateTimeOffset? ExpiresAt { get; set; }
    }
}
