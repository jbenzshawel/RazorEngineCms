using System.Data.Entity;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Practices.Unity;
using Unity.Mvc5;
using RazorEngineCms.DAL;
using RazorEngineCms.DAL.Repository;
using RazorEngineCms.Models;
using RazorEngineCms.Controllers;

namespace RazorEngineCms
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();
            
            // register Identity for Account Controller
            container.RegisterType<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>();
            container.RegisterType<UserManager<ApplicationUser>>();
            container.RegisterType<DbContext, ApplicationContext>();
            container.RegisterType<ApplicationUserManager>();
            container.RegisterType<AccountController>(new InjectionConstructor());
            // register RepostitoryService for page and include controllers
            container.RegisterType<IRepositoryService, RepositoryService>(new InjectionConstructor(new ApplicationContext()));
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}