using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using DeployTools.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeployTools.Web.Pages
{
    public class HostEditModel(IHostsService hostsService) : PageModel
    {
        [BindProperty]
        public Host Host { get; set; }

        [BindProperty]
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGet(string id)
        {
            Host = await hostsService.GetByIdAsync(id);
            if (Host == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var id = Request.Query["id"];

            var existingHost = await hostsService.GetByIdAsync(id);
            if (existingHost is null)
            {
                return NotFound();
            }

            Host.Id = id;
            
            var error = await Host.ValidateHost(hostsService);
            if (error is not null)
            {
                Host.Id = existingHost.Id;
                ErrorMessage = error;
                return Page();
            }

            existingHost.AssignedLoadBalancerArn = Host.AssignedLoadBalancerArn;
            existingHost.Address = Host.Address;
            existingHost.VpcId = Host.VpcId;
            existingHost.InstanceId = Host.InstanceId;
            existingHost.KeyFile = Host.KeyFile;
            existingHost.NextFreePort = Host.NextFreePort;
            existingHost.SshUserName = Host.SshUserName;

            await hostsService.UpdateAsync(existingHost);
            
            return RedirectToPage("/Hosts");
        }
    }
}