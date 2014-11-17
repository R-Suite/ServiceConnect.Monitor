using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace R.MessageBus.Monitor.Models
{
    public class Service
    {
        public ObjectId Id { get; set; }

        public List<string> InstanceLocation { get; set; } 
        public DateTime LastHeartbeat { get; set; }
        public string Name { get; set; }

        public List<string> In { get; set; }
        public List<string> Out { get; set; } 

        public List<ServiceMessage> MessagesIn { get; set; }
        public List<ServiceMessage> MessagesOut { get; set; } 

    }
}