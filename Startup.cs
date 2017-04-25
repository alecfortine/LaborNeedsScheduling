using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LaborNeedsScheduling.Startup))]
namespace LaborNeedsScheduling
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
