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
                name: "CMS",
                url: "CMS/Page/{action}/{section}/{name}/{param}/{param2}",
                defaults:
                new
                {
                    controller = "Page",
                    action = UrlParameter.Optional,
                    section = UrlParameter.Optional,
                    name = UrlParameter.Optional,
                    param = UrlParameter.Optional,
                    param2 = UrlParameter.Optional
                });

            routes.MapRoute(
                name: "Default",
                url: "{section}/{name}/{param}/{param2}",
                defaults: new
                {
                    controller = "Page",
                    action = "View",
                    name = UrlParameter.Optional,
                    section = UrlParameter.Optional,
                    param = UrlParameter.Optional,
                    param2 = UrlParameter.Optional
                });


        }
    }
}
