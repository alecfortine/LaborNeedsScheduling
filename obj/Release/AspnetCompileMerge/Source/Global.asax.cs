using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace LaborNeedsScheduling
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

           // RouteTable.Routes.MapPageRoute("DefaultRoute", string.Empty, "~Views/Home/Dashboard.cshtml");
        }
        void Session_Start(object sender, EventArgs e)
        {
            Session.Timeout = 525600;
        }
    }
}
