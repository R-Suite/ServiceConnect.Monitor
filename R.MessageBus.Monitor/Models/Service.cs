using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace R.MessageBus.Monitor.Models
{
    public class Service
    {
        public ObjectId Id { get; set; }
        public string InstanceLocation { get; set; } 
        public DateTime? LastHeartbeat { get; set; }
        public string Name { get; set; }
        public string Language { get; set; }
        public string ConsumerType { get; set; }
        public List<string> In { get; set; }
        public List<string> Out { get; set; } 
        public string Status { get; set; }
        public double LatestCpu { get; set; }
        public double LatestMemory { get; set; }
    }
}