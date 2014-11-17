using System.Web.Http;

namespace R.MessageBus.Monitor
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.EnableCors();
            config.MapHttpAttributeRoutes();
        }
    }
}
