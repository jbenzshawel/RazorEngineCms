using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using RazorEngineCms.ExtensionClasses;

namespace RazorEngineCms.Models
{
    public class TemplateModel
    {
        public ApplicationUser User { get; set; }

        public object PageModel { get; set; }

        public Include Includes { get; set; }

        public TemplateModel()
        {
            
            if (HttpContext.Current.Request.IsAuthenticated)
            {
                var db = new DAL.ApplicationContext();
                var userId = HttpContext.Current.User.Identity.GetUserId();
                var user = db.Users.FirstOrDefault(u => u.Id == userId);
                user.PasswordHash = null;
                user.SecurityStamp = null;
                this.User = user;
            }
        }

        public string ToJson(bool withPadding = false)
        {
            return this.ToJson<TemplateModel>(withPadding);
        }

        public string GetInclude(string name, string type)
        {
            return Include.GetContent(name, type);
        }
    }
}