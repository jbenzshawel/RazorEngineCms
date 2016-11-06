using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using RazorEngineCms.Models;

namespace RazorEngineCms.DAL
{
    public class ApplicationContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationContext()
            : base("ApplicationContext")
        {
        }
        
        public virtual IDbSet<Page> Page { get; set; }

        public virtual IDbSet<Include> Include { get; set; }

        public static ApplicationContext Create()
        {
            return new ApplicationContext();
        }
    }
}