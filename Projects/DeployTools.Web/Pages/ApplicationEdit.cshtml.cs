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
    public class ApplicationEditModel(IApplicationsService applicationsService, IPackagesService packagesService, IRdsPackagesService rdsPackagesService) : PageModel
    {
        [BindProperty] public Application Application { get; set; }
        [BindProperty] public string ErrorMessage { get; set; }
        public IList<Package> ValidPackages { get; set; }
        public IList<RdsPackage> ValidRdsPackages { get; set; }

        public async Task<IActionResult> OnGet(string id)
        {
            Application = await applicationsService.GetByIdAsync(id);
            if (Application is null)
            {
                return NotFound();
            }

            ValidPackages = await packagesService.GetAllAsync();
            ValidRdsPackages = await rdsPackagesService.GetAllAsync();

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var existingApplication = await applicationsService.GetByIdAsync(Request.Query["id"]);
            if (existingApplication is null)
            {
                return NotFound();
            }

            Application.Id = existingApplication.Id;
            Application.Name = existingApplication.Name;

            var error = await Application.ValidateApplication(applicationsService);
            if (error is not null)
            {
                ErrorMessage = error;
                return await OnGet(Request.Query["id"]);
            }

            existingApplication.Domain = Application.Domain;
            existingApplication.PackageId = Application.PackageId;
            existingApplication.RdsPackageId = Application.RdsPackageId.Equals("NONE", StringComparison.InvariantCultureIgnoreCase)
                    ? null
                    : Application.RdsPackageId;

            await applicationsService.UpdateAsync(existingApplication);
            
            return RedirectToPage("/Applications");
        }
    }
}