using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.DataAccess.Mappings
{
    public class RdsPackageMap : BaseMap<RdsPackage>
    {
        public RdsPackageMap()
        {
            Table("rds_packages");
            MapBase();

            Map(x => x.Name).Column("name");
            Map(x => x.DbInstance).Column("db_instance");
            Map(x => x.Engine).Column("engine");
            Map(x => x.EngineVersion).Column("engine_version");
            Map(x => x.StorageInGigabytes).Column("storage_in_gigabytes");
            Map(x => x.StorageType).Column("storage_type");
            Map(x => x.VpcId).Column("vpc_id");
            Map(x => x.VpcSecurityGroupId).Column("vpc_security_group_id");
        }
    }
}
