using System.Collections.Generic;
using Newtonsoft.Json;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Handlers
{
    public class HearbeatMessageHandler
    {
        private readonly IHeartbeatRepository _heartbeatRepository;
        private readonly object _lock = new object();

        public HearbeatMessageHandler(IHeartbeatRepository heartbeatRepository)
        {
            _heartbeatRepository = heartbeatRepository;
        }

        public void Execute(string message, IDictionary<string, string> headers)
        {
            var heartbeat = JsonConvert.DeserializeObject<Heartbeat>(message);

            lock (_lock)
            {
                _heartbeatRepository.InsertHeartbeat(heartbeat);
            }
        }
    }
}