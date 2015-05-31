using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Identity.Web.Startup))]
namespace Identity.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
