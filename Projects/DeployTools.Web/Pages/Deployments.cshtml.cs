using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeployTools.Web.Pages
{
    public class DeploymentsModel(IDeploymentsService deploymentsService) : PageModel
    {
        [BindProperty] public IList<ApplicationDeploy> Deployments { get; set; }

        public async Task<IActionResult> OnGet()
        {
            Deployments = await deploymentsService.GetAllDeploymentsAsync();

            return Page();
        }
    }
}
