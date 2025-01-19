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
    }

    public class ApplicationsModel(IApplicationsService applicationsService, 
        IPackagesService packagesService,
        IDeploymentsService deploymentsService) : PageModel
    {
        [BindProperty] public IList<ApplicationExtended> Applications { get; set; }
        [BindProperty] public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var applications = await applicationsService.GetAllAsync();
            var packages = await packagesService.GetAllAsync();

            Applications = new List<ApplicationExtended>();
            foreach (var application in applications)
            {
                var package = packages.First(x => x.Id == application.PackageId);
                Applications.Add(new()
                {
                    Id = application.Id,
                    Name = application.Name,
                    Domain = application.Domain,
                    PackageId = application.PackageId,
                    CreatedAt = application.CreatedAt,
                    PackageName = package.Name
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
