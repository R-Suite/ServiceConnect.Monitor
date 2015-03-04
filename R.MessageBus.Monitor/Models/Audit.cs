using System;
using MongoDB.Bson;
using System.Collections.Generic;

namespace R.MessageBus.Monitor.Models
{
    public class Audit
    {
        public ObjectId Id { get; set; }
        public DateTime TimeReceived { get; set; }
        public string DestinationMachine { get; set; }
        public DateTime TimeProcessed { get; set; }
        public string DestinationAddress { get; set; }
        public string MessageId { get; set; }
        public string SourceAddress { get; set; }
        public DateTime TimeSent { get; set; }
        public string SourceMachine { get; set; }
        public string MessageType { get; set; }
        public string FullTypeName { get; set; }
        public string TypeName { get; set; }
        public string Body { get; set; }
        public string ConsumerType { get; set; }
        public string Language { get; set; }
        public Guid CorrelationId { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        public string Server { get; set; }
    }
}