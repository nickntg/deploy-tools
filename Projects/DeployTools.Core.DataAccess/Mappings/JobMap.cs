using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.DataAccess.Mappings
{
    public class JobMap : BaseMap<Job>
    {
        public JobMap()
        {
            Table("jobs");
            MapBase();

            Map(x => x.IsProcessed).Column("is_processed");
            Map(x => x.SerializedInfo).Column("serialized_info");
            Map(x => x.Type).Column("type");
        }
    }
}
