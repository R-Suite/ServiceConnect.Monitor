//Copyright (C) 2015  Timothy Watson, Jakub Pachansky

//This program is free software; you can redistribute it and/or
//modify it under the terms of the GNU General Public License
//as published by the Free Software Foundation; either version 2
//of the License, or (at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using ServiceConnect.Monitor.Interfaces;
using ServiceConnect.Monitor.Models;

namespace ServiceConnect.Monitor.Handlers
{
    public class HearbeatMessageHandler : IDisposable
    {
        private readonly IHeartbeatRepository _heartbeatRepository;
        private readonly IHubContext _hub;
        private readonly Timer _timer;
        private readonly IList<Heartbeat> _heartbeats = new List<Heartbeat>();
        private readonly object _lock = new object();

        public HearbeatMessageHandler(IHeartbeatRepository heartbeatRepository, IHubContext hub)
        {
            _heartbeatRepository = heartbeatRepository;
            _hub = hub;
            var callback = new TimerCallback(SendHeartbeats);
            _timer = new Timer(callback, null, 0, 2500);
        }

        public async Task Execute(string message, IDictionary<string, string> headers, string host)
        {
            var heartbeat = JsonConvert.DeserializeObject<Heartbeat>(message);
            await _heartbeatRepository.InsertHeartbeat(heartbeat);

            lock (_lock)
                _heartbeats.Add(heartbeat);
        }

        private void SendHeartbeats(object state)
        {
            lock (_lock)
                if (_heartbeats.Count > 0)
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