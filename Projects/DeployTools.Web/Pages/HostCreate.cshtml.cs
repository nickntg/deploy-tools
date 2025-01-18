using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using DeployTools.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeployTools.Web.Pages
{
    public class HostCreateModel(IHostsService hostsService) : PageModel
    {
        [BindProperty]
        public Host Host { get; set; }

        [BindProperty]
        public string ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            Host = new Host();

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var error = await Host.ValidateHost(hostsService);
            if (error is not null)
            {
                ErrorMessage = error;
                return Page();
            }

            await hostsService.SaveAsync(Host);
            
            return RedirectToPage("/Hosts");
        }
    }
}