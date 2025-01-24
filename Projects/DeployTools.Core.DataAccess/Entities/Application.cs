namespace DeployTools.Core.DataAccess.Entities
{
    public class Application : BaseEntity
    {
        public virtual string Name { get; set; }
        public virtual string PackageId { get; set; }
        public virtual string Domain { get; set; }
        public virtual string RdsPackageId { get; set; }
    }
}
