namespace DeployTools.Core.DataAccess.Entities
{
    public class Job : BaseEntity
    {
        public virtual string Type { get; set; }
        public virtual string SerializedInfo { get; set; }
        public virtual bool IsProcessed { get; set; }
    }
}
