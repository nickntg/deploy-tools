using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Runtime.Internal;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeployTools.Web.Pages
{
    public class HostsModel(IHostsService hostsService, IDeploymentsService deploymentsService) : PageModel
    {
        [BindProperty] public IList<Host> Hosts { get; set; }
        [BindProperty] public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            Hosts = await hostsService.GetAllAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostDelete()
        {
            var id = Request.Form["id"];

            var host = await hostsService.GetByIdAsync(id);
            if (host is null)
            {
                return NotFound();
            }

            var deployments = await deploymentsService.GetActiveDeploymentsOfHostAsync(host.Id);
            if (deployments.Count > 0)
            {
                ErrorMessage = $"Host contains {deployments.Count} active deployments and cannot be deleted";
                return await OnGet();
            }

            await hostsService.DeleteAsync(host);

            return await OnGet();
        }
    }
}
