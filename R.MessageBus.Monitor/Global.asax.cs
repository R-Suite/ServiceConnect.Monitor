using System;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using R.MessageBus.Monitor.Handlers;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Repositories;
using StructureMap;

namespace R.MessageBus.Monitor
{
    public class WebApiApplication : HttpApplication
    {
        private Consumer _errorConsumer;
        private Consumer _auditConsumer;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var auditHandler = new AuditMessageHandler(ObjectFactory.GetInstance<IAuditRepository>());
            var errorHandler = new ErrorMessageHandler(ObjectFactory.GetInstance<IErrorRepository>());

            string host = WebConfigurationManager.AppSettings["Host"];
            string username = WebConfigurationManager.AppSettings["Username"];
            string password = WebConfigurationManager.AppSettings["Password"];
            _auditConsumer = new Consumer(host, username, password);
            _auditConsumer.StartConsuming(auditHandler.Execute, WebConfigurationManager.AppSettings["AuditQueue"]);
            _errorConsumer = new Consumer(host, username, password);
            _errorConsumer.StartConsuming(errorHandler.Execute, WebConfigurationManager.AppSettings["ErrorQueue"]);
        }

        protected void Application_End(object sender, EventArgs e)
        {
            _auditConsumer.Dispose();
            _errorConsumer.Dispose();
        }
    }
}