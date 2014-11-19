using System;
using System.Collections.Concurrent;
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
        private readonly Object _lock = new Object();
        private readonly IList<Error> _errors = new List<Error>();
        private readonly Timer _timer;

        public ErrorMessageHandler(IErrorRepository errorRepository, IHubContext hub)
        {
            _errorRepository = errorRepository;
            _hub = hub;
            var callback = new TimerCallback(SendErrors);
            _timer = new Timer(callback, null, 0, 200);
        }
        
        public void Execute(string message, IDictionary<string, string> headers)
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
                    Language = headers["Language"]
                };

                _errorRepository.InsertError(error);

                _errors.Add(error);
            }
        }

        private void SendErrors(object state)
        {
            lock (_errors)
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