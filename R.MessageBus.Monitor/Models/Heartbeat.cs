using System;
using MongoDB.Bson;

namespace R.MessageBus.Monitor.Models
{
    public class Heartbeat
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public double LatestCpu { get; set; }
        public double LatestMemory { get; set; }
        public string ConsumerType { get; set; }
        public string Language { get; set; }
        public DateTime Timestamp { get; set; }
        public ObjectId Id { get; set; }
    }
}