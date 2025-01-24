using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeployTools.Web.Pages
{
    public class RdsPackagesModel(IRdsPackagesService rdsPackagesService) : PageModel
    {
        [BindProperty] public IList<RdsPackage> RdsPackages { get; set; }
        [BindProperty] public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            RdsPackages = await rdsPackagesService.GetAllAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostDelete()
        {
            var id = Request.Form["id"];

            var rdsPackage = await rdsPackagesService.GetByIdAsync(id);
            if (rdsPackage is null)
            {
                return NotFound();
            }

            await rdsPackagesService.DeleteAsync(rdsPackage);

            return await OnGet();
        }
    }
}
