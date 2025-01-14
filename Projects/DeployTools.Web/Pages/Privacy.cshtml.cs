using System.Threading.Tasks;
using DeployTools.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeployTools.Web.Pages
{
    public class PrivacyModel(IHostsService hostsService) : PageModel
    {

        public async Task OnGet()
        {
            var host = await hostsService.GetByIdAsync("a");
        }
    }

}
