using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Owin;

namespace R.MessageBus.Monitor
{
    public class AuditConfig
    {
        public void Configuration(IAppBuilder app)
        {
            app.Map("/signalr", map =>
            {
                map.UseCors(CorsOptions.AllowAll);
                var hubConfiguration = new HubConfiguration();
                map.RunSignalR(hubConfiguration);
            });

            var httpConfig = new HttpConfiguration();
            app.UseWebApi(httpConfig);
            httpConfig.MapHttpAttributeRoutes();
            httpConfig.Routes.MapHttpRoute("Default", "{controller}/{action}", new { controller = "Home", action = "Index" });

            var appXmlType = httpConfig.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            httpConfig.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

            var cors = new EnableCorsAttribute("*", "*", "*");
            httpConfig.EnableCors(cors);
        }
    }
}