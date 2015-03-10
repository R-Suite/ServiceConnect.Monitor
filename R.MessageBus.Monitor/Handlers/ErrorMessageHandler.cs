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
using System.Globalization;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Handlers
{
    public class ErrorMessageHandler : IDisposable
    {
        private readonly IErrorRepository _errorRepository;
        private readonly IHubContext _hub;
        private readonly IList<Error> _errors = new List<Error>();
        private readonly Timer _timer;
        private readonly object _lock = new object();

        public ErrorMessageHandler(IErrorRepository errorRepository, IHubContext hub)
        {
            _errorRepository = errorRepository;
            _hub = hub;
            var callback = new TimerCallback(SendErrors);
            _timer = new Timer(callback, null, 0, 2500);
        }

        public void Execute(string message, IDictionary<string, string> headers, string host)
        {
            lock (_lock)
            {
                var error = new Error
                {
                    Body = message,
                    DestinationAddress = headers["DestinationAddress"],
                    DestinationMachine = headers["DestinationMachine"],
                    FullTypeName = headers["FullTypeName"],
                    MessageId = headers["MessageId"],
                    MessageType = headers["MessageType"],
                    SourceAddress = headers["SourceAddress"],
                    SourceMachine = headers["SourceMachine"],
                    TypeName = headers["TypeName"],
                    ConsumerType = headers["ConsumerType"],
                    TimeProcessed = DateTime.ParseExact(headers["TimeProcessed"], "O", CultureInfo.InvariantCulture),
                    TimeReceived = DateTime.ParseExact(headers["TimeReceived"], "O", CultureInfo.InvariantCulture),
                    TimeSent = DateTime.ParseExact(headers["TimeSent"], "O", CultureInfo.InvariantCulture),
                    Exception = JsonConvert.DeserializeObject<MessageException>(headers["Exception"]),
                    Language = headers["Language"],
                    CorrelationId = JsonConvert.DeserializeObject<Message>(message).CorrelationId,
                    Server = host,
                    Headers = headers
                };

                _errorRepository.InsertError(error);

                _errors.Add(error);
            }
        }

        private void SendErrors(object state)
        {
            lock (_lock)
            {
                if (_errors.Count > 0)
                {
                    _hub.Clients.All.Errors(_errors);
                    _errors.Clear();
                }
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}