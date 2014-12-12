using System;
using System.Collections.Generic;

namespace R.MessageBus.Monitor.Models
{
    public class Settings
    {
        public bool ForwardErrors { get; set; }
        public string ForwardErrorQueue { get; set; }
        public bool ForwardAudit { get; set; }
        public string ForwardAuditQueue { get; set; }
        public bool ForwardHeartbeats { get; set; }
        public string ForwardHeartbeatQueue { get; set; }
        public List<Environment> Environments { get; set; } 
        public string KeepErrorsFor { get; set; }
        public string KeepAuditsFor { get; set; }
        public string KeepHeartbeatsFor { get; set; }
        public Guid Id { get; set; }
    }
}