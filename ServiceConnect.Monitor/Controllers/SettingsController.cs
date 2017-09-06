//Copyright (C) 2015  Timothy Watson, Jakub Pachansky

//This program is free software; you can redistribute it and/or
//modify it under the terms of the GNU General Public License
//as published by the Free Software Foundation; either version 2
//of the License, or (at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.SignalR;
using ServiceConnect.Monitor.Handlers;
using ServiceConnect.Monitor.Hubs;
using ServiceConnect.Monitor.Interfaces;
using ServiceConnect.Monitor.Models;

namespace ServiceConnect.Monitor.Controllers
{
    public class SettingsController : ApiController
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly IAuditRepository _auditRepository;
        private readonly IErrorRepository _errorRepository;
        private readonly IHeartbeatRepository _heartbeatRepository;

        public SettingsController(ISettingsRepository settingsRepository, IAuditRepository auditRepository, IErrorRepository errorRepository, IHeartbeatRepository heartbeatRepository)
        {
            _settingsRepository = settingsRepository;
            _auditRepository = auditRepository;
            _errorRepository = errorRepository;
            _heartbeatRepository = heartbeatRepository;
        }

        [AcceptVerbs("GET")]
        [Route("settings")]
        public async Task<Settings> GetSettings()
        {
            return await _settingsRepository.Get();
        }

        [AcceptVerbs("POST", "PUT")]
        [Route("settings")]
        public async Task<Settings> UpdateSettings(Settings model)
        {
            if (string.IsNullOrEmpty(model.KeepAuditsFor))
                model.KeepAuditsFor = "Forever";

            if (string.IsNullOrEmpty(model.KeepErrorsFor))
                model.KeepErrorsFor = "Forever";

            if (string.IsNullOrEmpty(model.KeepHeartbeatsFor))
                model.KeepHeartbeatsFor = "Forever";

            await _settingsRepository.Update(model);

            if (model.ForwardAudit == false)
                foreach (var consumerEnvironment in Globals.Environments)
                    consumerEnvironment.AuditConsumer.SetForwardQueue(null);

            if (Globals.Settings.ForwardAuditQueue != model.ForwardAuditQueue)
                foreach (var consumerEnvironment in Globals.Environments)
                    consumerEnvironment.AuditConsumer.SetForwardQueue(model.ForwardAuditQueue);

            if (model.ForwardErrors == false)
                foreach (var consumerEnvironment in Globals.Environments)
                    consumerEnvironment.ErrorConsumer.SetForwardQueue(null);

            if (Globals.Settings.ForwardErrorQueue != model.ForwardErrorQueue)
                foreach (var consumerEnvironment in Globals.Environments)
                    consumerEnvironment.ErrorConsumer.SetForwardQueue(model.ForwardErrorQueue);

            if (model.ForwardHeartbeats == false)
                foreach (var consumerEnvironment in Globals.Environments)
                    consumerEnvironment.HeartbeatConsumer.SetForwardQueue(null);

            if (Globals.Settings.ForwardHeartbeatQueue != model.ForwardHeartbeatQueue)
                foreach (var consumerEnvironment in Globals.Environments)
                    consumerEnvironment.HeartbeatConsumer.SetForwardQueue(model.ForwardHeartbeatQueue);

            Globals.Settings = model;

            var auditHub = GlobalHost.ConnectionManager.GetHubContext<AuditHub>();
            var errorHub = GlobalHost.ConnectionManager.GetHubContext<ErrorHub>();
            var heartbeatHub = GlobalHost.ConnectionManager.GetHubContext<HeartbeatHub>();

            var environments = Globals.Environments;
            foreach (var environment in model.Environments)
            {
                var consumerEnvironment = environments.FirstOrDefault(x => x.Server == environment.Server &&
                                                                           x.Username == environment.Username &&
                                                                           x.Password == environment.Password &&
                                                                           x.SslEnabled == environment.SslEnabled &&
                                                                           x.CertBase64 == environment.CertBase64 &&
                                                                           x.CertPassword == environment.CertPassword &&
                                                                           x.AuditQueue == environment.AuditQueue &&
                                                                           x.ErrorQueue == environment.ErrorQueue &&
                                                                           x.HeartbeatQueue == environment.HeartbeatQueue);
                if (consumerEnvironment == null)
                {
                    var receivingConnection = new Connection(environment);
                    var sendingConnection = new Connection(environment);

                    consumerEnvironment = new ConsumerEnvironment
                    {
                        Server = environment.Server,
                        AuditMessageHandler = new AuditMessageHandler(_auditRepository, auditHub),
                        ErrorMessageHandler = new ErrorMessageHandler(_errorRepository, errorHub),
                        HeartbeatMessageHandler = new HearbeatMessageHandler(_heartbeatRepository, heartbeatHub),
                        ReceivingConnection = receivingConnection,
                        SendingConnection = sendingConnection,
                        AuditConsumer = new ConsumerPool(receivingConnection),
                        ErrorConsumer = new ConsumerPool(receivingConnection),
                        HeartbeatConsumer = new ConsumerPool(receivingConnection),
                        Producer = new Producer(sendingConnection)
                    };

                    string forwardErrorQueue = null;
                    string forwardAuditQueue = null;
                    string forwardHeartbeatQueue = null;

                    if (Globals.Settings.ForwardAudit)
                        forwardAuditQueue = Globals.Settings.ForwardAuditQueue;

                    if (Globals.Settings.ForwardErrors)
                        forwardErrorQueue = Globals.Settings.ForwardErrorQueue;

                    if (Globals.Settings.ForwardHeartbeats)
                        forwardHeartbeatQueue = Globals.Settings.ForwardHeartbeatQueue;

                    consumerEnvironment.AuditConsumer.StartConsuming(consumerEnvironment.AuditMessageHandler.Execute, environment.AuditQueue, forwardAuditQueue);
                    consumerEnvironment.ErrorConsumer.StartConsuming(consumerEnvironment.ErrorMessageHandler.Execute, environment.ErrorQueue, forwardErrorQueue);
                    consumerEnvironment.HeartbeatConsumer.StartConsuming(consumerEnvironment.HeartbeatMessageHandler.Execute, environment.HeartbeatQueue, forwardHeartbeatQueue);
                }
            }

            var environmentsToRemove = new List<ConsumerEnvironment>();
            foreach (var consumerEnvironment in environments)
            {
                var environment = model.Environments.FirstOrDefault(x => x.Server == consumerEnvironment.Server &&
                                                                         x.Username == consumerEnvironment.Username &&
                                                                         x.Password == consumerEnvironment.Password &&
                                                                         x.SslEnabled == consumerEnvironment.SslEnabled &&
                                                                         x.CertBase64 == consumerEnvironment.CertBase64 &&
                                                                         x.CertPassword == consumerEnvironment.CertPassword &&
                                                                         x.AuditQueue == consumerEnvironment.AuditQueue &&
                                                                         x.ErrorQueue == consumerEnvironment.ErrorQueue &&
                                                                         x.HeartbeatQueue == consumerEnvironment.HeartbeatQueue);
                if (environment == null)
                    environmentsToRemove.Add(consumerEnvironment);
            }

            foreach (var consumerEnvironment in environmentsToRemove)
            {
                consumerEnvironment.AuditMessageHandler.Dispose();
                consumerEnvironment.ErrorMessageHandler.Dispose();
                consumerEnvironment.HeartbeatMessageHandler.Dispose();
                consumerEnvironment.AuditConsumer.Dispose();
                consumerEnvironment.ErrorConsumer.Dispose();
                consumerEnvironment.HeartbeatConsumer.Dispose();
                consumerEnvironment.Producer.Dispose();
                consumerEnvironment.ReceivingConnection.Dispose();
                consumerEnvironment.SendingConnection.Dispose();
                environments.Remove(consumerEnvironment);
            }
             
            return model;
        }
    }
}