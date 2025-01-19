using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeployTools.Web.Pages
{
    public class ActiveDeploymentsModel(IHostsService hostsService, 
        IPackagesService packagesService,
        IApplicationsService applicationsService,
        IDeploymentsService deploymentsService) : PageModel
    {
        [BindProperty] public IList<ActiveDeploymentExtended> ActiveDeployments { get; set; }
        [BindProperty] public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var activeDeployments = await deploymentsService.GetAllActiveDeploymentsAsync();

            ActiveDeployments = new List<ActiveDeploymentExtended>();

            var hosts = await hostsService.GetAllAsync();
            var packages = await packagesService.GetAllAsync();
            var applications = await applicationsService.GetAllAsync();

            foreach (var deployment in activeDeployments)
            {
                var o = new ActiveDeploymentExtended
                {
                    Id = deployment.Id,
                    CreatedAt = deployment.CreatedAt,
                    PackageId = deployment.PackageId,
                    HostId = deployment.HostId,
                    Port = deployment.Port,
                    ApplicationId = deployment.ApplicationId,
                    DeployId = deployment.DeployId,
                    HostAddress = hosts.First(x => x.Id == deployment.HostId).Address,
                    PackageName = packages.First(x => x.Id == deployment.PackageId).Name,
                    ApplicationName = applications.First(x => x.Id == deployment.ApplicationId).Name
                };

                ActiveDeployments.Add(o);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostTakeDown()
        {
            var id = Request.Form["id"];

            var deployment = await deploymentsService.GetActiveDeploymentByIdAsync(id);
            if (deployment is null)
            {
                return NotFound();
            }

            await deploymentsService.StartTakeDownAsync(id);

            return await OnGet();
        }
    }

    public class ActiveDeploymentExtended : ActiveDeployment
    {
        public string ApplicationName { get; set; }
        public string HostAddress { get; set; }
        public string PackageName { get; set; }
    }
}
