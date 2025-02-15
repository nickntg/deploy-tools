using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using DeployTools.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeployTools.Web.Pages
{
    public class CertificateCreateModel(ICertificatesService certificatesService) : PageModel
    {
        [BindProperty]
        public Certificate Certificate { get; set; }

        [BindProperty]
        public string ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            Certificate = new Certificate();

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var error = await Certificate.ValidateCertificate(certificatesService);
            if (error is not null)
            {
                ErrorMessage = error;
                return Page();
            }

            Certificate.IsCreated = false;
            Certificate.IsValidated = false;

            await certificatesService.SaveAsync(Certificate);
            
            return RedirectToPage("/Certificates");
        }
    }
}