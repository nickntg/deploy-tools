using System.Collections.Generic;
using System.Threading.Tasks;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeployTools.Web.Pages
{
    public class DeployJournalModel(IDeploymentsService deploymentsService) : PageModel
    {
        [BindProperty] public IList<JournalEntry> JournalEntries { get; set; }

        public async Task<IActionResult> OnGet(string id)
        {
            JournalEntries = await deploymentsService.GetJournalEntriesOfDeployAsync(id);

            return Page();
        }
    }
}