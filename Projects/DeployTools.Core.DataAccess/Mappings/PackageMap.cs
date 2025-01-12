using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.DataAccess.Mappings
{
    public class PackageMap : BaseMap<Package>
    {
        public PackageMap()
        {
            Table("packages");
            MapBase();

            Map(x => x.DeployableLocation).Column("deployable_location");
            Map(x => x.ExecutableFile).Column("executable_file");
            Map(x => x.Name).Column("name");
        }
    }
}
