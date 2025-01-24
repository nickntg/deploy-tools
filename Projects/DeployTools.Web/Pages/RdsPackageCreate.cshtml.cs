using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using DeployTools.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeployTools.Web.Pages
{
    public class RdsPackageCreateModel(IRdsPackagesService rdsPackagesService) : PageModel
    {
        [BindProperty]
        public RdsPackage RdsPackage { get; set; }

        [BindProperty]
        public string ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            RdsPackage = new RdsPackage();

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var error = await RdsPackage.ValidateRdsPackage(rdsPackagesService);
            if (error is not null)
            {
                ErrorMessage = error;
                return Page();
            }

            await rdsPackagesService.SaveAsync(RdsPackage);
            
            return RedirectToPage("/RdsPackages");
        }
    }
}