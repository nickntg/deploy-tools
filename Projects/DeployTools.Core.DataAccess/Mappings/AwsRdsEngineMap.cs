using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.DataAccess.Mappings
{
    public class AwsRdsEngineMap : BaseMap<AwsRdsEngine>
    {
        public AwsRdsEngineMap()
        {
            Table("rds_engines");
            MapBase();

            Map(x => x.EngineName).Column("engine_name");
            Map(x => x.EngineVersion).Column("engine_version");
            Map(x => x.Status).Column("status");
        }
    }
}
