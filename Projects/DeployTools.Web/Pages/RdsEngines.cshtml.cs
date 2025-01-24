using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.RDS;
using Amazon.RDS.Model;
using DeployTools.Core.DataAccess.Entities;
using DeployTools.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeployTools.Web.Pages
{
    public class RdsEnginesModel(IRdsEnginesService rdsEnginesService) : PageModel
    {
        [BindProperty] public IList<AwsRdsEngine> Engines { get; set; }
        [BindProperty] public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            Engines = await rdsEnginesService.GetAllAsync();

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            //var o = new AmazonRDSClient();
            //var x = o.DescribeDBInstancesAsync();
            //x.Result.DBInstances[]
            //o.CreateDBClusterAsync(new CreateDBClusterRequest
            //{
            //    sto
            //})
            await rdsEnginesService.RefreshAsync();

            return await OnGet();
        }
    }
}
