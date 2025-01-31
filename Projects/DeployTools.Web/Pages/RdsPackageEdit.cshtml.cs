using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using DeployTools.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeployTools.Web.Pages
{
    public class RdsPackageEditModel(IRdsPackagesService rdsPackagesService) : PageModel
    {
        [BindProperty]
        public RdsPackage RdsPackage { get; set; }

        [BindProperty]
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGet(string id)
        {
            RdsPackage = await rdsPackagesService.GetByIdAsync(id);
            if (RdsPackage is null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var existingPackage = await rdsPackagesService.GetByIdAsync(Request.Query["id"]);
            if (existingPackage is null)
            {
                return NotFound();
            }

            RdsPackage.Id = existingPackage.Id;
            RdsPackage.Name = existingPackage.Name;

            var error = await RdsPackage.ValidateRdsPackage(rdsPackagesService);
            if (error is not null)
            {
                RdsPackage.Id = existingPackage.Id;
                ErrorMessage = error;
                return Page();
            }

            existingPackage.DbInstance = RdsPackage.DbInstance;
            existingPackage.Engine = RdsPackage.Engine;
            existingPackage.EngineVersion = RdsPackage.EngineVersion;
            existingPackage.StorageInGigabytes = RdsPackage.StorageInGigabytes;
            existingPackage.StorageType = RdsPackage.StorageType;
            existingPackage.VpcId = RdsPackage.VpcId;
            existingPackage.VpcSecurityGroupId = RdsPackage.VpcSecurityGroupId;

            await rdsPackagesService.UpdateAsync(existingPackage);
            
            return RedirectToPage("/RdsPackages");
        }
    }
}