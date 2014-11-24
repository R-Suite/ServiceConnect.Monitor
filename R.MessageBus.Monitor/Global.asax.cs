using System;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using R.MessageBus.Monitor.Handlers;
using R.MessageBus.Monitor.Interfaces;
using StructureMap;

namespace R.MessageBus.Monitor
{
    public class WebApiApplication : HttpApplication
    {
        private Consumer _errorConsumer;
        private Consumer _auditConsumer;
        private Consumer _heartbeatConsumer;
        private AuditMessageHandler _auditHandler;
        private ErrorMessageHandler _errorHandler;
        private HearbeatMessageHandler _heartbeatHandler;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            _auditHandler = new AuditMessageHandler(ObjectFactory.GetInstance<IAuditRepository>());
            _errorHandler = new ErrorMessageHandler(ObjectFactory.GetInstance<IErrorRepository>());
            _heartbeatHandler = new HearbeatMessageHandler(ObjectFactory.GetInstance<IHeartbeatRepository>());

            string host = WebConfigurationManager.AppSettings["Host"];
            string username = WebConfigurationManager.AppSettings["Username"];
            string password = WebConfigurationManager.AppSettings["Password"];
            _auditConsumer = new Consumer(host, username, password);
            _auditConsumer.StartConsuming(_auditHandler.Execute, WebConfigurationManager.AppSettings["AuditQueue"]);
            _errorConsumer = new Consumer(host, username, password);
            _errorConsumer.StartConsuming(_errorHandler.Execute, WebConfigurationManager.AppSettings["ErrorQueue"]);
            _heartbeatConsumer = new Consumer(host, username, password);
            _heartbeatConsumer.StartConsuming(_heartbeatHandler.Execute, WebConfigurationManager.AppSettings["HeartbeatQueue"]);
        }

        protected void Application_End(object sender, EventArgs e)
        {
            _auditConsumer.Dispose();
            _errorConsumer.Dispose();
            _heartbeatConsumer.Dispose();
        }
    }
}