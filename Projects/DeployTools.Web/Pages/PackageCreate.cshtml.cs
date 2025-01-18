using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using DeployTools.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeployTools.Web.Pages
{
    public class PackageCreateModel(IPackagesService packagesService) : PageModel
    {
        [BindProperty] public Package Package { get; set; }
        [BindProperty] public string ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            Package = new Package();

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var error = await Package.ValidatePackage(packagesService);
            if (error is not null)
            {
                ErrorMessage = error;
                return Page();
            }

            await packagesService.SaveAsync(Package);

            await packagesService.UpdateAsync(Package);
            
            return RedirectToPage("/Packages");
        }
    }
}