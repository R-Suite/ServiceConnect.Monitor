using System;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.SignalR;
using R.MessageBus.Monitor.Handlers;
using R.MessageBus.Monitor.Hubs;
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

            var auditHub = GlobalHost.ConnectionManager.GetHubContext<AuditHub>();
            var errorHub = GlobalHost.ConnectionManager.GetHubContext<ErrorHub>();
            var heartbeatHub = GlobalHost.ConnectionManager.GetHubContext<HeartbeatHub>();

            _auditHandler = new AuditMessageHandler(ObjectFactory.GetInstance<IAuditRepository>(), auditHub);
            _errorHandler = new ErrorMessageHandler(ObjectFactory.GetInstance<IErrorRepository>(), errorHub);
            _heartbeatHandler = new HearbeatMessageHandler(ObjectFactory.GetInstance<IHeartbeatRepository>(), heartbeatHub);

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
            _auditHandler.Dispose();
            _errorHandler.Dispose();
            _heartbeatConsumer.Dispose();
            _auditConsumer.Dispose();
            _errorConsumer.Dispose();
            _heartbeatConsumer.Dispose();
        }
    }
}