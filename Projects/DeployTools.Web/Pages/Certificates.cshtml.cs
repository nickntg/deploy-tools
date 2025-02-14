using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeployTools.Web.Pages
{
    public class CertificatesModel(ICertificatesService certificatesService, 
        IApplicationsService applicationsService,
        IDeploymentsService deploymentsService) : PageModel
    {
        [BindProperty] public IList<Certificate> Certificates { get; set; }
        [BindProperty] public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            Certificates = await certificatesService.GetAllAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostDelete()
        {
            var id = Request.Form["id"];

            var certificate = await certificatesService.GetByIdAsync(id);
            if (certificate is null)
            {
                return NotFound();
            }

            var applications = await applicationsService.GetApplicationsByDomainAsync(certificate.Domain);
            if (applications.Count is > 0)
            {
                foreach (var application in applications)
                {
                    var deployed = await deploymentsService.GetActiveDeploymentsOfApplicationAsync(application.Id);
                    if (deployed.Count is > 0)
                    {
                        ErrorMessage =
                            $"Cannot delete certificate, in use by deployed application {application.Name} in {deployed.Count} deployments";
                        return await OnGet();
                    }
                }
            }

            await certificatesService.DeleteAsync(certificate);

            return await OnGet();
        }
    }
}
