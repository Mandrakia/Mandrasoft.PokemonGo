
using Hangfire.Dashboard;
using Microsoft.Owin;
using System.Collections.Generic;
using Hangfire.Annotations;
using System;

public class MyRestrictiveAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize([NotNull] DashboardContext context)
    {
        return true;
    }
}