namespace DeployTools.Core.DataAccess.Entities
{
    public class ActiveDeployment : BaseEntity
    {
        public virtual string ApplicationId { get; set; }
        public virtual string HostId { get; set; }
        public virtual string PackageId { get; set; }
        public virtual string DeployId { get; set; }
    }
}
