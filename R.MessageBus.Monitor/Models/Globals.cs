using System.Collections.Generic;
using System.Threading;

namespace R.MessageBus.Monitor.Models
{
    public static class Globals 
    {
        public static readonly List<ConsumerEnvironment> Environments = new List<ConsumerEnvironment>();
        public static readonly List<Timer> Timers = new List<Timer>();
        public static Settings Settings { get; set; }
    }
}