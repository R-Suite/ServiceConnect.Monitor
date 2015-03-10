using Microsoft.Owin;
using Owin;
using R.MessageBus.Monitor.App_Start;

[assembly: OwinStartup(typeof(Startup))]
namespace R.MessageBus.Monitor.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}