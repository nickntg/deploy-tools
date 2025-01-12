namespace DeployTools.Core.DataAccess.Entities
{
    public class Package : BaseEntity
    {
        public virtual string Name { get; set; }
        public virtual string DeployableLocation { get; set; }
        public virtual string ExecutableFile { get; set; }
    }
}
