using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.DataAccess.Mappings
{
    public class ApplicationMap : BaseMap<Application>
    {
        public ApplicationMap()
        {
            Table("applications");
            MapBase();

            Map(x => x.Name).Column("name");
            Map(x => x.PackageId).Column("package_id");
            Map(x => x.Domain).Column("domain");
        }
    }
}
