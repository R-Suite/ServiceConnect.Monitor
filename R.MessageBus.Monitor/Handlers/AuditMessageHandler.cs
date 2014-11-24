using System;
using System.Collections.Generic;
using System.Globalization;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Handlers
{
    public class AuditMessageHandler
    {
        private readonly IAuditRepository _auditRepository;
        private readonly object _lock = new object();

        public AuditMessageHandler(IAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public void Execute(string message, IDictionary<string, string> headers)
        {
            lock (_lock)
            {
                var audit = new Audit
                {
                    Body = message,
                    DestinationAddress = headers["DestinationAddress"],
                    DestinationMachine = headers["DestinationMachine"],
                    FullTypeName = headers["FullTypeName"],
                    MessageId = headers["MessageId"],
                    MessageType = headers["MessageType"],
                    SourceAddress = headers["SourceAddress"],
                    SourceMachine = headers["SourceMachine"],
                    TypeName = headers["TypeName"],
                    ConsumerType = headers["ConsumerType"],
                    TimeProcessed = DateTime.ParseExact(headers["TimeProcessed"], "O", CultureInfo.InvariantCulture),
                    TimeReceived = DateTime.ParseExact(headers["TimeReceived"], "O", CultureInfo.InvariantCulture),
                    TimeSent = DateTime.ParseExact(headers["TimeSent"], "O", CultureInfo.InvariantCulture),
                    Language = headers["Language"]
                };

                _auditRepository.InsertAudit(audit);
            }
        }
    }
}