using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.SignalR;
using R.MessageBus.Monitor.Handlers;
using R.MessageBus.Monitor.Hubs;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;
using StructureMap;

namespace R.MessageBus.Monitor.Controllers
{
    public class SettingsController : ApiController
    {
        private readonly ISettingsRepository _settingsRepository;

        public SettingsController(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        [AcceptVerbs("GET")]
        [Route("settings")]
        public Settings GetSettings()
        {
            return _settingsRepository.Get();
        }

        [AcceptVerbs("POST", "PUT")]
        [Route("settings")]
        public Settings UpdateSettings(Settings model)
        {
            if (string.IsNullOrEmpty(model.KeepAuditsFor))
            {
                model.KeepAuditsFor = "Forever";
            }
            if (string.IsNullOrEmpty(model.KeepErrorsFor))
            {
                model.KeepErrorsFor = "Forever";
            }
            if (string.IsNullOrEmpty(model.KeepHeartbeatsFor))
            {
                model.KeepHeartbeatsFor = "Forever";
            }

            _settingsRepository.Update(model);

            if (model.ForwardAudit == false)
            {
                foreach (ConsumerEnvironment consumerEnvironment in Globals.Environments)
                {
                    consumerEnvironment.AuditConsumer.SetForwardQueue(null);
                }
            }
            if (Globals.Settings.ForwardAuditQueue != model.ForwardAuditQueue)
            {
                foreach (ConsumerEnvironment consumerEnvironment in Globals.Environments)
                {
                    consumerEnvironment.AuditConsumer.SetForwardQueue(model.ForwardAuditQueue);
                }
            }

            if (model.ForwardErrors == false)
            {
                foreach (ConsumerEnvironment consumerEnvironment in Globals.Environments)
                {
                    consumerEnvironment.ErrorConsumer.SetForwardQueue(null);
                }
            }
            if (Globals.Settings.ForwardErrorQueue != model.ForwardErrorQueue)
            {
                foreach (ConsumerEnvironment consumerEnvironment in Globals.Environments)
                {
                    consumerEnvironment.ErrorConsumer.SetForwardQueue(model.ForwardErrorQueue);
                }
            }

            if (model.ForwardHeartbeats == false)
            {
                foreach (ConsumerEnvironment consumerEnvironment in Globals.Environments)
                {
                    consumerEnvironment.HeartbeatConsumer.SetForwardQueue(null);
                }
            }
            if (Globals.Settings.ForwardHeartbeatQueue != model.ForwardHeartbeatQueue)
            {
                foreach (ConsumerEnvironment consumerEnvironment in Globals.Environments)
                {
                    consumerEnvironment.HeartbeatConsumer.SetForwardQueue(model.ForwardHeartbeatQueue);
                }
            }

            Globals.Settings = model;

            var auditHub = GlobalHost.ConnectionManager.GetHubContext<AuditHub>();
            var errorHub = GlobalHost.ConnectionManager.GetHubContext<ErrorHub>();
            var heartbeatHub = GlobalHost.ConnectionManager.GetHubContext<HeartbeatHub>();

            var environments = Globals.Environments;
            foreach (Environment environment in model.Environments)
            {
                var consumerEnvironment = environments.FirstOrDefault(x => x.Server == environment.Server &&
                                                                           x.Username == environment.Username &&
                                                                           x.Password == environment.Password &&
                                                                           x.AuditQueue == environment.AuditQueue &&
                                                                           x.ErrorQueue == environment.ErrorQueue &&
                                                                           x.HeartbeatQueue == environment.HeartbeatQueue);
                if (consumerEnvironment == null)
                {
                    consumerEnvironment = new ConsumerEnvironment
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

                    if (Globals.Settings.ForwardAudit)
                    {
                        forwardAuditQueue = Globals.Settings.ForwardAuditQueue;
                    }
                    if (Globals.Settings.ForwardErrors)
                    {
                        forwardErrorQueue = Globals.Settings.ForwardErrorQueue;
                    }
                    if (Globals.Settings.ForwardHeartbeats)
                    {
                        forwardHeartbeatQueue = Globals.Settings.ForwardHeartbeatQueue;
                    }

                    consumerEnvironment.AuditConsumer.StartConsuming(consumerEnvironment.AuditMessageHandler.Execute, environment.AuditQueue, forwardAuditQueue);
                    consumerEnvironment.ErrorConsumer.StartConsuming(consumerEnvironment.ErrorMessageHandler.Execute, environment.ErrorQueue, forwardErrorQueue);
                    consumerEnvironment.HeartbeatConsumer.StartConsuming(consumerEnvironment.HeartbeatMessageHandler.Execute, environment.HeartbeatQueue, forwardHeartbeatQueue);
                }
            }

            var environmentsToRemove = new List<ConsumerEnvironment>();
            foreach (ConsumerEnvironment consumerEnvironment in environments)
            {
                var environment = model.Environments.FirstOrDefault(x => x.Server == consumerEnvironment.Server &&
                                                                           x.Username == consumerEnvironment.Username &&
                                                                           x.Password == consumerEnvironment.Password &&
                                                                           x.AuditQueue == consumerEnvironment.AuditQueue &&
                                                                           x.ErrorQueue == consumerEnvironment.ErrorQueue &&
                                                                           x.HeartbeatQueue == consumerEnvironment.HeartbeatQueue);
                if (environment == null)
                {
                    environmentsToRemove.Add(consumerEnvironment);
                }
            }

            foreach (ConsumerEnvironment consumerEnvironment in environmentsToRemove)
            {
                consumerEnvironment.AuditConsumer.Dispose();
                consumerEnvironment.ErrorConsumer.Dispose();
                consumerEnvironment.HeartbeatConsumer.Dispose();
                consumerEnvironment.AuditMessageHandler.Dispose();
                consumerEnvironment.ErrorMessageHandler.Dispose();
                consumerEnvironment.HeartbeatMessageHandler.Dispose();
                environments.Remove(consumerEnvironment);
            }
             
            return model;
        }
    }
}
