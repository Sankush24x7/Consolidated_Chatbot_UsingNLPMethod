using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Consolidated_ChatBot
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            var context = HttpContext.Current;

            // Session is NOW available
            if (context.Session == null) return;

            if (context.Session["USER_ID"] == null &&
                context.Request.Cookies["REMEMBER_ME"] != null)
            {
                var c = context.Request.Cookies["REMEMBER_ME"];

                context.Session["USER_ID"] = c["USER_ID"];
                context.Session["USERNAME"] = c["USERNAME"];
                context.Session["ROLE"] = c["ROLE"];
            }
        }


    }
}
