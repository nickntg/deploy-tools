namespace DeployTools.Core.DataAccess.Entities
{
    public class AwsRdsEngine : BaseEntity
    {
        public virtual string EngineName { get; set; }
        public virtual string EngineVersion { get; set; }
        public virtual string Status { get; set; }
    }
}
