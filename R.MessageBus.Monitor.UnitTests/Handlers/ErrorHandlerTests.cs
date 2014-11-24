using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNet.SignalR;
using Moq;
using Newtonsoft.Json;
using R.MessageBus.Monitor.Handlers;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;
using Xunit;

namespace R.MessageBus.Monitor.UnitTests.Handlers
{
    public class ErrorHandlerTests
    {
        private readonly Mock<IErrorRepository> _mockRepository;
        private readonly Dictionary<string, string> _headers;
        private readonly MessageException _messageException;

        public ErrorHandlerTests()
        {
            _mockRepository = new Mock<IErrorRepository>();
            _mockRepository.Setup(x => x.InsertError(It.IsAny<Error>()));

            _messageException = new MessageException
            {
                Timestamp = DateTime.UtcNow,
                Exception = "Exception",
                ExceptionType = "ExceptionType",
                Message = "Message",
                Source = "Source",
                StackTrace = "StackTrace"
            };

            _headers = new Dictionary<string, string>
            {
                { "TimeReceived", DateTime.UtcNow.AddSeconds(-30).ToString("O") },
                { "DestinationMachine", "DestinationMachine" },
                { "TimeProcessed", DateTime.UtcNow.ToString("O") },
                { "DestinationAddress", "DestinationAddress" },
                { "MessageId", Guid.NewGuid().ToString() },
                { "SourceAddress", "SourceAddress" },
                { "TimeSent", DateTime.UtcNow.AddMinutes(-1).ToString("O")},
                { "SourceMachine", "SourceMachine" },
                { "MessageType", "MessageType" },
                { "FullTypeName", "FullTypeName" },
                { "TypeName", "TypeName" },
                { "Exception", JsonConvert.SerializeObject(_messageException) },
                { "ConsumerType", "RabbitMQ"},
                { "Language", "C#"}
            };
        }

        [Fact]
        public void ShouldInsertAuditMessageAndHeadersIntoRepository()
        {
            // Arrange
            var handler = new ErrorMessageHandler(_mockRepository.Object);

            // Act
            handler.Execute("TestMessage", _headers);

            // Assert
            _mockRepository.Verify(x => x.InsertError(It.Is<Error>(m =>
                m.DestinationAddress == _headers["DestinationAddress"] &&
                m.DestinationMachine == _headers["DestinationMachine"] &&
                m.FullTypeName == _headers["FullTypeName"] &&
                m.MessageId == _headers["MessageId"] &&
                m.MessageType == _headers["MessageType"] &&
                m.SourceAddress == _headers["SourceAddress"] &&
                m.SourceMachine == _headers["SourceMachine"] &&
                m.TypeName == _headers["TypeName"] &&
                m.TimeProcessed == DateTime.ParseExact(_headers["TimeProcessed"], "O", CultureInfo.InvariantCulture) &&
                m.TimeReceived == DateTime.ParseExact(_headers["TimeReceived"], "O", CultureInfo.InvariantCulture) &&
                m.TimeSent == DateTime.ParseExact(_headers["TimeSent"], "O", CultureInfo.InvariantCulture) &&
                m.Body == "TestMessage" &&
                m.Exception.Exception == _messageException.Exception &&
                m.Exception.ExceptionType == _messageException.ExceptionType &&
                m.Exception.Message == _messageException.Message &&
                m.Exception.Source == _messageException.Source &&
                m.Exception.StackTrace == _messageException.StackTrace &&
                m.Exception.Timestamp == _messageException.Timestamp &&
                m.ConsumerType == "RabbitMQ" &&
                m.Language == "C#"
            )), Times.Once());
        } 
    }
}