using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WorkTracker.Startup))]
namespace WorkTracker
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
