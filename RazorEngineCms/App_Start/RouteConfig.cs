using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RazorEngineCms
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Page",
                url: "Page/{action}/{name}/{section}",
                defaults:
                new
                {
                    controller = "Page",
                    action = UrlParameter.Optional,
                    name = UrlParameter.Optional,
                    section = UrlParameter.Optional
                });

            routes.MapRoute(
                name: "CMS",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{page}/{section}/{param}/{param2}",
                defaults: new
                {
                    controller = "Page",
                    action = "View",
                    name = UrlParameter.Optional,
                    variable = UrlParameter.Optional,
                    param = UrlParameter.Optional,
                    param2 = UrlParameter.Optional
                });


        }
    }
}
