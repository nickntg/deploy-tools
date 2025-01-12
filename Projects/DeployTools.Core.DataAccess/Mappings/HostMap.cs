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
        }
    }
}
