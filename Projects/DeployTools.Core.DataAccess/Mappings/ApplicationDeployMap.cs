using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.DataAccess.Mappings
{
    public class ApplicationDeployMap : BaseMap<ApplicationDeploy>
    {
        public ApplicationDeployMap()
        {
            Table("application_deploys");
            MapBase();

            Map(x => x.HostId).Column("host_id");
            Map(x => x.ApplicationId).Column("application_id");
            Map(x => x.IsSuccessful).Column("is_successful");
            Map(x => x.IsCompleted).Column("is_completed");
        }
    }
}
