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