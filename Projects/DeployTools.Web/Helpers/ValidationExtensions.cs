using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;

namespace DeployTools.Web.Helpers
{
    public static class ValidationExtensions
    {
        public static async Task<string> ValidateCertificate(this Certificate certificate,
            ICertificatesService certificateService)
        {
            try
            {
                CheckEmpty(certificate.Domain, "Domain cannot be empty");
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            var existing = await certificateService.GetCertificateByDomainAsync(certificate.Domain);
            if (existing is not null)
            {
                return "Certificate with the same domain already exists";
            }

            return null;
        }

        public static async Task<string> ValidateRdsPackage(this RdsPackage package, IRdsPackagesService rdsPackageService)
        {
            try
            {
                CheckEmpty(package.Name, "Name cannot be empty");
                CheckEmpty(package.Engine, "Engine cannot be empty");
                CheckEmpty(package.EngineVersion, "Engine version cannot be empty");
                CheckEmpty(package.DbInstance, "DB instance cannot be empty");
                CheckEmpty(package.VpcId, "VPC ID cannot be empty");
                CheckEmpty(package.VpcSecurityGroupId, "VPC security group ID cannot be empty");
                CheckEmpty(package.DbSubnetGroupName, "DB subnet group name cannot be empty");

                if (!string.IsNullOrEmpty(package.StorageType))
                {
                    if (package.StorageInGigabytes is null or < 20)
                    {
                        throw new ValidationException("Storage must be over 20 if it is specified");
                    }
                }
            }
            catch (ValidationException ex)
            {
                return ex.Message;
            }

            var existing = await rdsPackageService.GetPackagesByNameAsync(package.Name);
            if (existing.Count > 1 || (existing.Count == 1 && existing[0].Id != package.Id))
            {
                return "Package with the same name already exists";
            }

            return null;
        }

        public static async Task<string> ValidateApplication(this Application application,
            IApplicationsService applicationsService)
        {
            try
            {
                CheckEmpty(application.Name, "Name cannot be empty");
                CheckEmpty(application.Domain, "Domain cannot be empty");
            }
            catch (ValidationException ex)
            {
                return ex.Message;
            }

            var existing = await applicationsService.GetApplicationsByNameAsync(application.Name);
            if (existing.Count > 1 || (existing.Count == 1 && existing[0].Id != application.Id))
            {
                return "Application with the same name already exists";
            }

            existing = await applicationsService.GetApplicationsByDomainAsync(application.Domain);
            if (existing.Count > 1 || (existing.Count == 1 && existing[0].Id != application.Id))
            {
                return "Application with the same domain already exists";
            }

            if (!string.IsNullOrEmpty(application.Id))
            {
                var db = await applicationsService.GetByIdAsync(application.Id);
                if (db is null)
                {
                    return "Cannot find application to edit";
                }

                if (db.Name != application.Name)
                {
                    return "Once set, application name cannot be changed";
                }
            }

            return null;
        }
        public static async Task<string> ValidatePackage(this Package package, IPackagesService packagesService)
        {
            try
            {
                CheckEmpty(package.DeployableLocation, "Deployable location cannot be empty");
                CheckEmpty(package.ExecutableFile, "Executable file cannot be empty");
                CheckEmpty(package.Name, "Package name cannot be empty");
            }
            catch (ValidationException ex)
            {
                return ex.Message;
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
            try
            {
                CheckEmpty(host.AssignedLoadBalancerArn, "Assigned load balancer cannot be empty");
                CheckEmpty(host.Address, "Address cannot be empty");
                CheckEmpty(host.InstanceId, "Instance id cannot be empty");
                CheckEmpty(host.KeyFile, "Key file cannot be empty");
                CheckEmpty(host.SshUserName, "SSH user name cannot be empty");
                CheckEmpty(host.VpcId, "VPC id cannot be empty");

            }
            catch (ValidationException ex)
            {
                return ex.Message;
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

        private static void CheckEmpty(string value, string errorMessage)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ValidationException(errorMessage);
            }
        }
    }
}
