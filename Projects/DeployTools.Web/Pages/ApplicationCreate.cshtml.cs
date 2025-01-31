using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using DeployTools.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeployTools.Web.Pages
{
    public class ApplicationCreateModel(IApplicationsService applicationsService, IPackagesService packagesService, IRdsPackagesService rdsPackagesService) : PageModel
    {
        [BindProperty] public Application Application { get; set; }
        [BindProperty] public string ErrorMessage { get; set; }
        public IList<Package> ValidPackages { get; set; }
        public IList<RdsPackage> ValidRdsPackages { get; set; }

        public async Task<IActionResult> OnGet()
        {
            Application = new Application();

            ValidPackages = await packagesService.GetAllAsync();
            ValidRdsPackages = await rdsPackagesService.GetAllAsync();

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

            if (Application.RdsPackageId.Equals("NONE", StringComparison.CurrentCultureIgnoreCase))
            {
                Application.RdsPackageId = null;
            }

            await applicationsService.SaveAsync(Application);
            
            return RedirectToPage("/Applications");
        }
    }
}