using System.Collections.Generic;
using System.Threading;

namespace R.MessageBus.Monitor.Models
{
    public static class Globals 
    {
        public static readonly List<ConsumerEnvironment> Environments = new List<ConsumerEnvironment>();
        public static readonly Dictionary<string, Timer> Timers = new Dictionary<string, Timer>();
        public static Settings Settings { get; set; }
        public static string ErrorExpiry { get; set; }
        public static string HeartbeatExpiry { get; set; }
        public static string AuditExpiry { get; set; }
    }
}