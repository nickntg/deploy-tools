namespace DeployTools.Core.DataAccess.Entities
{
    public class ApplicationDeploy : BaseEntity
    {
        public virtual string ApplicationId { get; set; }
        public virtual string HostId { get; set; }
        public virtual bool IsSuccessful { get; set; }
        public virtual bool IsCompleted { get; set; }
    }
}
