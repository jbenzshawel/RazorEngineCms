using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RazorEngineCms.App_Classes
{
    public class AuthRedirect : ActionFilterAttribute, IActionFilter
    {

        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!System.Web.HttpContext.Current.Request.IsAuthenticated)
            {
                string actionName = filterContext.RouteData.Values["action"].ToString();
                //string controllerName = filterContext.RouteData.Values["controller"].ToString();
                //string successRedirect = actionName;

                filterContext.Result = new RedirectToRouteResult(
                    "LoginRedirect",
                    new RouteValueDictionary(new {
                        controller = "Account",
                        action = "Login",
                        returnUrl = actionName
                    }));
            }
        }
    }
}