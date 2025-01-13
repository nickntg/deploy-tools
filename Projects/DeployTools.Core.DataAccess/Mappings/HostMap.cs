using DeployTools.Core.DataAccess.Entities;

namespace DeployTools.Core.DataAccess.Mappings
{
    public class HostMap : BaseMap<Host>
    {
        public HostMap()
        {
            Table("hosts");
            MapBase();

            Map(x => x.Address).Column("address");
            Map(x => x.SshUserName).Column("ssh_user_name");
            Map(x => x.KeyFile).Column("key_file");
            Map(x => x.NextFreePort).Column("next_free_port");
            Map(x => x.InstanceId).Column("instance_id");
            Map(x => x.AssignedLoadBalancerArn).Column("assigned_load_balancer_arn");
            Map(x => x.VpcId).Column("vpc_id");
        }
    }
}
