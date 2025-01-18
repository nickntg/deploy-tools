using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeployTools.Web.Pages
{
    public class PackagesModel(IPackagesService packagesService, IDeploymentsService deploymentsService) : PageModel
    {
        [BindProperty] public IList<Package> Packages { get; set; }
        [BindProperty] public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            Packages = await packagesService.GetAllAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostDelete()
        {
            var id = Request.Form["id"];

            var package = await packagesService.GetByIdAsync(id);
            if (package is null)
            {
                return NotFound();
            }

            var deployments = await deploymentsService.GetActiveDeploymentsOfPackageAsync(package.Id);
            if (deployments.Count > 0)
            {
                ErrorMessage = $"Package deployed {deployments.Count} times in active deployments and cannot be deleted";
                return await OnGet();
            }

            await packagesService.DeleteAsync(package);

            return await OnGet();
        }
    }
}
