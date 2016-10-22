using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using RazorEngineCms.DAL;
using Newtonsoft.Json;

namespace RazorEngineCms.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        private static ApplicationContext _db = new ApplicationContext();
        

        public string ToJson()
        {
            // remove sensitive info before adding exposing as JSON 
            this.PasswordHash = null;
            this.SecurityStamp = null;
            this.Id = null;
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}