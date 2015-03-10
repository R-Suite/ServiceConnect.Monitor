using System;
using MongoDB.Bson;

namespace R.MessageBus.Monitor.Models
{
    public class ServiceMessage
    {
        public ObjectId Id { get; set; }

        public string In { get; set; }
        public string Out { get; set; }
        public string Type { get; set; }
        public int Count { get; set; }
        public DateTime LastSent { get; set; }
        public int ErrorCount { get; set; }
        public MessageException LastError { get; set; }
    }
}