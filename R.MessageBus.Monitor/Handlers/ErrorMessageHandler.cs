using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Handlers
{
    public class ErrorMessageHandler
    {
        private readonly IErrorRepository _errorRepository;
        private readonly object _lock = new object();

        public ErrorMessageHandler(IErrorRepository errorRepository)
        {
            _errorRepository = errorRepository;
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
            }
        }
    }
}