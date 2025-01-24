using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeployTools.Web.Pages
{
    public class ApplicationExtended : Application
    {
        public string PackageName { get; set; }
        public string RdsPackageName { get; set; }
    }

    public class ApplicationsModel(IApplicationsService applicationsService, 
        IPackagesService packagesService,
        IRdsPackagesService rdsPackagesService,
        IDeploymentsService deploymentsService) : PageModel
    {
        [BindProperty] public IList<ApplicationExtended> Applications { get; set; }
        [BindProperty] public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var applications = await applicationsService.GetAllAsync();
            var packages = await packagesService.GetAllAsync();
            var rdsPackages = await rdsPackagesService.GetAllAsync();

            Applications = new List<ApplicationExtended>();
            foreach (var application in applications)
            {
                var package = packages.First(x => x.Id == application.PackageId);
                var rdsPackageName = string.IsNullOrEmpty(application.RdsPackageId)
                    ? string.Empty
                    : rdsPackages.First(x => x.Id == application.RdsPackageId).Name;
                Applications.Add(new()
                {
                    Id = application.Id,
                    Name = application.Name,
                    Domain = application.Domain,
                    PackageId = application.PackageId,
                    CreatedAt = application.CreatedAt,
                    PackageName = package.Name,
                    RdsPackageName = rdsPackageName
                });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDelete()
        {
            var id = Request.Form["id"];

            var application = await applicationsService.GetByIdAsync(id);
            if (application is null)
            {
                return NotFound();
            }

            var deployments = await deploymentsService.GetActiveDeploymentsOfApplicationAsync(application.Id);
            if (deployments.Count > 0)
            {
                ErrorMessage = $"Application is deployed in {deployments.Count} active deployments and cannot be deleted";
                return await OnGet();
            }

            await applicationsService.DeleteAsync(application);

            return await OnGet();
        }
    }
}
