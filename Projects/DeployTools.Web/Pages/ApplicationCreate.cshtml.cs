using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using DeployTools.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeployTools.Web.Pages
{
    public class ApplicationCreateModel(IApplicationsService applicationsService, IPackagesService packagesService) : PageModel
    {
        [BindProperty] public Application Application { get; set; }
        [BindProperty] public string ErrorMessage { get; set; }
        public IList<Package> ValidPackages { get; set; }

        public async Task<IActionResult> OnGet()
        {
            Application = new Application();

            ValidPackages = await packagesService.GetAllAsync();

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var error = await Application.ValidateApplication(applicationsService);
            if (error is not null)
            {
                ErrorMessage = error;
                return Page();
            }

            await applicationsService.SaveAsync(Application);
            
            return RedirectToPage("/Applications");
        }
    }
}