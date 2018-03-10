using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RentNiceCar.Startup))]
namespace RentNiceCar
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
