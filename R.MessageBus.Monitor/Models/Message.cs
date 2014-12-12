using System;

namespace R.MessageBus.Monitor.Models
{
    public class Message
    {
        public Guid CorrelationId { get; set; }
    }
}