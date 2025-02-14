using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeployTools.Web.Pages
{
    public class ApplicationDeployModel(IHostsService hostsService, 
        IApplicationsService applicationsService,
        IDeploymentsService deploymentsService,
        ICertificatesService certificatesService): PageModel
    {
        [BindProperty] public ApplicationDeployExtended ApplicationDeploy { get; set; }
        [BindProperty] public string ErrorMessage { get; set; }
        public IList<Host> Hosts { get; set; }

        public async Task<IActionResult> OnGet(string id)
        {
            var application = await applicationsService.GetByIdAsync(id);
            if (application is null)
            {
                return NotFound();
            }
            
            Hosts = await hostsService.GetAllAsync();

            ApplicationDeploy = new ApplicationDeployExtended
            {
                ApplicationName = application.Name
            };

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var id = Request.Query["id"];

            var application = await applicationsService.GetByIdAsync(id);
            if (application is null)
            {
                return NotFound();
            }

            var certificate = await certificatesService.GetCertificateByDomainAsync(application.Domain);
            if (certificate is null)
            {
                ErrorMessage = "No certificate found for application domain";
                return await OnGet(id);
            }

            await deploymentsService.StartDeploymentAsync(id, ApplicationDeploy.HostId);

            return RedirectToPage("/Applications");
        }
    }

    public class ApplicationDeployExtended
    {
        public string ApplicationName { get; set; }
        public string HostId { get; set; }
    }
}
