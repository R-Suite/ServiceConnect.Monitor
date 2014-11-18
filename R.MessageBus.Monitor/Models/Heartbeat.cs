using System;

namespace R.MessageBus.Monitor.Models
{
    public class Heartbeat
    {
        public string ServiceName { get; set; }
        public string Location { get; set; }
        public decimal CpuUsage { get; set; }
        public decimal MemoryUsage { get; set; }
        public string ConsumerType { get; set; }
        public string Language { get; set; }
        public DateTime Timestamp { get; set; }
    }
}