using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Unity.Mvc5;
using RazorEngineCms.DAL;
using RazorEngineCms.DAL.Repository;

namespace RazorEngineCms
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();
            container.RegisterType<IRepositoryService, RepositoryService>(new InjectionConstructor(new ApplicationContext()));
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}