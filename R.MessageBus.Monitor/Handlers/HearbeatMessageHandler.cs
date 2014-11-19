using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Handlers
{
    public class HearbeatMessageHandler : IDisposable
    {
        private readonly IHeartbeatRepository _heartbeatRepository;
        private readonly IHubContext _hub;
        private readonly Object _lock = new Object();
        private readonly Timer _timer;
        private readonly IList<Heartbeat> _heartbeats = new List<Heartbeat>();

        public HearbeatMessageHandler(IHeartbeatRepository heartbeatRepository, IHubContext hub)
        {
            _heartbeatRepository = heartbeatRepository;
            _hub = hub;
            var callback = new TimerCallback(SendHeartbeats);
            _timer = new Timer(callback, null, 0, 200);
        }

        public void Execute(string message, IDictionary<string, string> headers)
        {
            var heartbeat = JsonConvert.DeserializeObject<Heartbeat>(message);

            lock (_lock)
            {
                _heartbeatRepository.InsertHeartbeat(heartbeat);
                _heartbeats.Add(heartbeat);
            }
        }

        private void SendHeartbeats(object state)
        {
            lock (_heartbeats)
            {
                _hub.Clients.All.Heartbeats(_heartbeats);
                _heartbeats.Clear();
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}