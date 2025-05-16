using Hangfire.Dashboard;

namespace Api.DashboardAuthorizationFilters;

public class DashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        try
        {
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}