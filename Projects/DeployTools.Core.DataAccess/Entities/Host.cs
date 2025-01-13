namespace DeployTools.Core.DataAccess.Entities
{
    public class Host : BaseEntity
    {
        public virtual string Address { get; set; }
        public virtual string InstanceId { get; set; }
        public virtual string SshUserName { get; set; }
        public virtual string KeyFile { get; set; }
        public virtual int NextFreePort { get; set; }
        public virtual string AssignedLoadBalancerArn { get; set; }
        public virtual string VpcId { get; set; }
    }
}
