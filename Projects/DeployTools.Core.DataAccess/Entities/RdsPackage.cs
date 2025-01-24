namespace DeployTools.Core.DataAccess.Entities
{
    public class RdsPackage : BaseEntity
    {
        public virtual string Name { get; set; }
        public virtual string Engine { get; set; }
        public virtual string EngineVersion { get; set; }
        public virtual string DbInstance { get; set; }
        public virtual string StorageType { get; set; }
        public virtual int? StorageInGigabytes { get; set; }
        public virtual string VpcId { get; set; }
        public virtual string VpcSecurityGroupId { get; set; }
    }
}
