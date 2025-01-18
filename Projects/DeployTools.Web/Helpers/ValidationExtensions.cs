using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;

namespace DeployTools.Web.Helpers
{
    public static class ValidationExtensions
    {
        public static async Task<string> ValidatePackage(this Package package, IPackagesService packagesService)
        {
            if (string.IsNullOrEmpty(package.DeployableLocation))
            {
                return "Deployable location cannot be empty";
            }

            if (string.IsNullOrEmpty(package.ExecutableFile))
            {
                return "Executable file cannot be empty";
            }

            if (string.IsNullOrEmpty(package.Name))
            {
                return "Package name cannot be empty";
            }

            var existing = await packagesService.GetPackagesByNameAsync(package.Name);
            if (existing.Count > 1 || (existing.Count == 1 && existing[0].Id != package.Id))
            {
                return "Package with the same name already exists";
            }

            existing = await packagesService.GetPackagesByDeployableLocationAsync(package.DeployableLocation);
            if (existing.Count > 1 || (existing.Count == 1 && existing[0].Id != package.Id))
            {
                return "Package with the same deployable already exists";
            }

            return null;
        }

        public static async Task<string> ValidateHost(this Host host, IHostsService hostsService)
        {
            if (string.IsNullOrEmpty(host.AssignedLoadBalancerArn))
            {
                return "Assigned load balancer cannot be empty";
            }

            if (string.IsNullOrEmpty(host.Address))
            {
                return "Address cannot be empty";
            }

            if (string.IsNullOrEmpty(host.InstanceId))
            {
                return "Instance id cannot be empty";
            }

            if (string.IsNullOrEmpty(host.KeyFile))
            {
                return "Key file cannot be empty";
            }

            if (string.IsNullOrEmpty(host.SshUserName))
            {
                return "SSH user name cannot be empty";
            }

            if (string.IsNullOrEmpty(host.VpcId))
            {
                return "VPC id cannot be empty";
            }

            var existing = await hostsService.GetHostsByAddress(host.Address);
            if (existing.Count > 1 || (existing.Count == 1 && existing[0].Id != host.Id))
            {
                return "Host with the same address already exists";
            }

            existing = await hostsService.GetHostsByInstanceId(host.InstanceId);
            if (existing.Count > 1 || (existing.Count == 1 && existing[0].Id != host.Id))
            {
                return "Host with the same instance id already exists";
            }

            return null;
        }
    }
}
