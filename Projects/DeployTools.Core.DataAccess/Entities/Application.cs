namespace DeployTools.Core.DataAccess.Entities
{
    public class Application : BaseEntity
    {
        public virtual string Name { get; set; }
        public virtual string HostId { get; set; }
        public virtual string PackageId { get; set; }
        public virtual int Port { get; set; }
    }
}
