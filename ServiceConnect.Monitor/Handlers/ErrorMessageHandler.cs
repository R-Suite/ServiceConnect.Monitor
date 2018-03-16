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
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using ServiceConnect.Monitor.Interfaces;
using ServiceConnect.Monitor.Models;

namespace ServiceConnect.Monitor.Handlers
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

        public async Task Execute(string message, IDictionary<string, string> headers, string host)
        {
            try
            {
                Guid? correlationId = null;
                try
                {
                    correlationId = JsonConvert.DeserializeObject<Message>(message).CorrelationId;
                }
                catch
                { }

                MessageException exception = null;
                try
                {
                    exception = JsonConvert.DeserializeObject<MessageException>(headers["Exception"]);
                }
                catch { }

                var error = new Error
                {
                    Body = message,
                    DestinationAddress = headers["DestinationAddress"],
                    DestinationMachine = headers["DestinationMachine"],
                    FullTypeName = headers.ContainsKey("FullTypeName") ? headers["FullTypeName"] : null,
                    MessageId = headers.ContainsKey("MessageId") ? headers["MessageId"] : null,
                    MessageType = headers.ContainsKey("MessageType") ? headers["MessageType"] : null,
                    SourceAddress = headers.ContainsKey("SourceAddress") ? headers["SourceAddress"] : null,
                    SourceMachine = headers.ContainsKey("SourceMachine") ? headers["SourceMachine"] : null,
                    TypeName = headers.ContainsKey("TypeName") ? headers["TypeName"] : null,
                    ConsumerType = headers.ContainsKey("ConsumerType") ? headers["ConsumerType"] : null,
                    TimeProcessed = headers.ContainsKey("TimeProcessed") ? DateTime.ParseExact(headers["TimeProcessed"], "O", CultureInfo.InvariantCulture) : DateTime.MinValue,
                    TimeReceived = headers.ContainsKey("TimeReceived") ? DateTime.ParseExact(headers["TimeReceived"], "O", CultureInfo.InvariantCulture) : DateTime.MinValue,
                    TimeSent = headers.ContainsKey("TimeSent") ? DateTime.ParseExact(headers["TimeSent"], "O", CultureInfo.InvariantCulture) : DateTime.MinValue,
                    Language = headers.ContainsKey("Language") ? headers["Language"] : null,
                    CorrelationId = correlationId,
                    Exception = exception,
                    Server = host,
                    Headers = headers
                };

                await _errorRepository.InsertError(error);

                lock (_lock)
                    _errors.Add(error);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SendErrors(object state)
        {
            lock (_lock)
                if (_errors.Count > 0)
                {
                    _hub.Clients.All.Errors(_errors);
                    _errors.Clear();
                }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}