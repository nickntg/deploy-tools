using Hangfire.Dashboard;

namespace DeployTools.Batch.Hangfire
{
	public class AllAuthorizationFilter : IDashboardAuthorizationFilter
	{
		public bool Authorize(DashboardContext context)
		{
			return true;
		}
	}
}