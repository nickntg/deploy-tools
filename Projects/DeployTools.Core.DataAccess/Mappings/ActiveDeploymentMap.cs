using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.DataAccess.Mappings
{
    public class ActiveDeploymentMap : BaseMap<ActiveDeployment>
    {
        public ActiveDeploymentMap()
        {
            Table("active_deployments");
            MapBase();

            Map(x => x.PackageId).Column("package_id");
            Map(x => x.ApplicationId).Column("application_id");
            Map(x => x.HostId).Column("host_id");
            Map(x => x.DeployId).Column("deploy_id");
        }
    }
}
