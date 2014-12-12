using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.SignalR;
using R.MessageBus.Monitor.Handlers;
using R.MessageBus.Monitor.Hubs;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;
using StructureMap;
using Environment = R.MessageBus.Monitor.Models.Environment;

namespace R.MessageBus.Monitor
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var auditHub = GlobalHost.ConnectionManager.GetHubContext<AuditHub>();
            var errorHub = GlobalHost.ConnectionManager.GetHubContext<ErrorHub>();
            var heartbeatHub = GlobalHost.ConnectionManager.GetHubContext<HeartbeatHub>();

            var settingsRepository  = ObjectFactory.GetInstance<ISettingsRepository>();
            var settings = settingsRepository.Get();
            if (settings == null)
            {
                settings = new Settings
                {
                    Id = Guid.NewGuid(),
                    Environments = new List<Environment>(),
                    KeepAuditsFor = "7",
                    KeepErrorsFor = "Forever",
                    KeepHeartbeatsFor = "7"
                };
                settingsRepository.Update(settings);
            }

            Globals.Settings = settings;
            
            foreach (Environment environment in settings.Environments)
            {
                var consumerEnvironment = new ConsumerEnvironment
                {
                    Server = environment.Server,
                    AuditMessageHandler = new AuditMessageHandler(ObjectFactory.GetInstance<IAuditRepository>(), auditHub),
                    ErrorMessageHandler = new ErrorMessageHandler(ObjectFactory.GetInstance<IErrorRepository>(), errorHub),
                    HeartbeatMessageHandler = new HearbeatMessageHandler(ObjectFactory.GetInstance<IHeartbeatRepository>(), heartbeatHub),
                    AuditConsumer = new Consumer(environment.Server, environment.Username, environment.Password),
                    ErrorConsumer = new Consumer(environment.Server, environment.Username, environment.Password),
                    HeartbeatConsumer = new Consumer(environment.Server, environment.Username, environment.Password)
                };
                string forwardErrorQueue = null;
                string forwardAuditQueue = null;
                string forwardHeartbeatQueue = null;

                if (settings.ForwardAudit)
                {
                    forwardAuditQueue = settings.ForwardAuditQueue;
                }
                if (settings.ForwardErrors)
                {
                    forwardErrorQueue = settings.ForwardErrorQueue;
                }
                if (settings.ForwardHeartbeats)
                {
                    forwardHeartbeatQueue = settings.ForwardHeartbeatQueue;
                }

                consumerEnvironment.AuditConsumer.StartConsuming(consumerEnvironment.AuditMessageHandler.Execute, environment.AuditQueue, forwardAuditQueue);
                consumerEnvironment.ErrorConsumer.StartConsuming(consumerEnvironment.ErrorMessageHandler.Execute, environment.ErrorQueue, forwardErrorQueue);
                consumerEnvironment.HeartbeatConsumer.StartConsuming(consumerEnvironment.HeartbeatMessageHandler.Execute, environment.HeartbeatQueue, forwardHeartbeatQueue);

                Globals.Environments.Add(consumerEnvironment);
            }

            var timer = new Timer(AuditCallback, settings, 0, 300000);
            Globals.Timers.Add(timer);
            timer = new Timer(ErrorCallback, settings, 0, 300000);
            Globals.Timers.Add(timer);
            timer = new Timer(HeartbeatCallback, settings, 0, 300000);
            Globals.Timers.Add(timer);

            var auditRepository = ObjectFactory.GetInstance<IAuditRepository>();
            auditRepository.EnsureIndex();
            var errorRepository = ObjectFactory.GetInstance<IErrorRepository>();
            errorRepository.EnsureIndex();
            var heartbeatRepository = ObjectFactory.GetInstance<IHeartbeatRepository>();
            heartbeatRepository.EnsureIndex();
            var serviceMessageRepository = ObjectFactory.GetInstance<IServiceMessageRepository>();
            serviceMessageRepository.EnsureIndex();
            var serviceRepository = ObjectFactory.GetInstance<IServiceRepository>();
            serviceRepository.EnsureIndex();
        }

        private void AuditCallback(object state)
        {
            var period = ((Settings) state).KeepAuditsFor;
            if (!string.IsNullOrEmpty(period) && period != "Forever")
            {
                var repository = ObjectFactory.GetInstance<IAuditRepository>();
                repository.Remove(DateTime.Now.AddDays(Convert.ToInt32(period) * -1));
            }
        }

        private void ErrorCallback(object state)
        {
            var period = ((Settings)state).KeepErrorsFor;
            if (!string.IsNullOrEmpty(period) && period != "Forever")
            {
                var repository = ObjectFactory.GetInstance<IErrorRepository>();
                repository.Remove(DateTime.Now.AddDays(Convert.ToInt32(period) * -1));
            }
        }

        private void HeartbeatCallback(object state)
        {
            var period = ((Settings)state).KeepHeartbeatsFor;
            if (!string.IsNullOrEmpty(period) && period != "Forever")
            {
                var repository = ObjectFactory.GetInstance<IHeartbeatRepository>();
                repository.Remove(DateTime.Now.AddDays(Convert.ToInt32(period) * -1));
            }
        }

        protected void Application_End(object sender, EventArgs e)
        {
            foreach (ConsumerEnvironment environment in Globals.Environments)
            {
                environment.AuditMessageHandler.Dispose();
                environment.ErrorMessageHandler.Dispose();
                environment.HeartbeatMessageHandler.Dispose();
                environment.AuditConsumer.Dispose();
                environment.ErrorConsumer.Dispose();
                environment.HeartbeatConsumer.Dispose();
            }

            foreach (Timer timer in Globals.Timers)
            {
                timer.Dispose();
            }
        }
    }
}