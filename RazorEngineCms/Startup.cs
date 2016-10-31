using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RazorEngineCms.Startup))]
namespace RazorEngineCms
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
