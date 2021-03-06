﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WorkTracker
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "DashboardFilters",
                url: "Home/Dashboard/{userFilter}/{paidFilter}",
                defaults: new { controller = "Home", action = "Dashboard", userFilter = UrlParameter.Optional, paidFilter = UrlParameter.Optional}
            );

            routes.MapRoute(
                name: "PaymentReportFilters",
                url: "Home/PaymentReport/{userFilter}",
                defaults: new { controller = "Home", action = "PaymentReport", userFilter = UrlParameter.Optional}
            );

            routes.MapRoute(
                name: "DeleteEmployee",
                url: "Home/DeleteUser/{userId}",
                defaults: new {controller = "Home", action = "DeleteUser"}
             );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

           
        }
    }
}
