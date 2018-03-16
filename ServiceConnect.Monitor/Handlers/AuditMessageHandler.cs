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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using ServiceConnect.Monitor.Interfaces;
using ServiceConnect.Monitor.Models;

namespace ServiceConnect.Monitor.Handlers
{
    public class AuditMessageHandler : IDisposable
    {
        private readonly IAuditRepository _auditRepository;
        private readonly IHubContext _hub;
        private readonly Timer _timer;
        private readonly IList<Audit> _audits = new List<Audit>();
        private readonly object _lock = new object();

        public AuditMessageHandler(IAuditRepository auditRepository, IHubContext hub)
        {
            _auditRepository = auditRepository;
            _hub = hub;
            var callback = new TimerCallback(SendAudits);
            _timer = new Timer(callback, null, 0, 2500);
        }

        public async Task Execute(string message, IDictionary<string, string> headers, string host)
        {
            try
            {
                Guid? correlationId = null;
                try
                {
                    correlationId = JsonConvert.DeserializeObject<Message>(message).CorrelationId;
                }
                catch
                { }

                var audit = new Audit
                {
                    Body = message,
                    DestinationAddress = headers["DestinationAddress"],
                    DestinationMachine = headers["DestinationMachine"],
                    FullTypeName = headers.ContainsKey("FullTypeName") ? headers["FullTypeName"] : null,
                    MessageId = headers.ContainsKey("MessageId") ? headers["MessageId"] : null,
                    MessageType = headers.ContainsKey("MessageType") ? headers["MessageType"] : null,
                    SourceAddress = headers.ContainsKey("SourceAddress") ? headers["SourceAddress"] : null,
                    SourceMachine = headers.ContainsKey("SourceMachine") ? headers["SourceMachine"] : null,
                    TypeName = headers.ContainsKey("TypeName") ? headers["TypeName"] : null,
                    ConsumerType = headers.ContainsKey("ConsumerType") ? headers["ConsumerType"] : null,
                    TimeProcessed = headers.ContainsKey("TimeProcessed") ? DateTime.Parse(headers["TimeProcessed"]) : DateTime.MinValue,
                    TimeReceived = headers.ContainsKey("TimeReceived") ? DateTime.Parse(headers["TimeReceived"]) : DateTime.MinValue,
                    TimeSent = headers.ContainsKey("TimeSent") ? DateTime.Parse(headers["TimeSent"]) : DateTime.MinValue,
                    Language = headers.ContainsKey("Language") ? headers["Language"] : null,
                    CorrelationId = correlationId,
                    Server = host,
                    Headers = headers
                };

                await _auditRepository.InsertAudit(audit);

                lock (_lock)
                    _audits.Add(audit);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SendAudits(object state)
        {
            lock (_lock)
                if (_audits.Count > 0)
                {
                    _hub.Clients.All.Audits(_audits);
                    _audits.Clear();
                }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}