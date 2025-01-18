using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using DeployTools.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeployTools.Web.Pages
{
    public class PackageEditModel(IPackagesService packagesService) : PageModel
    {
        [BindProperty] public Package Package { get; set; }
        [BindProperty] public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGet(string id)
        {
            Package = await packagesService.GetByIdAsync(id);
            if (Package == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var existingPackage = await packagesService.GetByIdAsync(Request.Query["id"]);
            if (existingPackage is null)
            {
                return NotFound();
            }

            var error = await Package.ValidatePackage(packagesService);
            if (error is not null)
            {
                Package.Id = existingPackage.Id;
                ErrorMessage = error;
                return Page();
            }

            existingPackage.DeployableLocation = Package.DeployableLocation;
            existingPackage.ExecutableFile = Package.ExecutableFile;
            existingPackage.Name = Package.Name;

            await packagesService.UpdateAsync(Package);
            
            return RedirectToPage("/Packages");
        }
    }
}