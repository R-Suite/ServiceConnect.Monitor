using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNet.SignalR;
using Moq;
using R.MessageBus.Monitor.Handlers;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;
using Xunit;

namespace R.MessageBus.Monitor.UnitTests.Handlers
{
    public class AuditHandlerTests
    {
        private readonly Mock<IAuditRepository> _mockRepository;
        private readonly Dictionary<string, string> _headers;
        private Mock<IHubContext> _mockContext;

        public AuditHandlerTests()
        {
            _mockRepository = new Mock<IAuditRepository>();
            _mockRepository.Setup(x => x.InsertAudit(It.IsAny<Audit>()));
            _mockContext = new Mock<IHubContext>();

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
                { "ConsumerType", "RabbitMQ"},
                { "Language", "C#"}
            };
        }

        [Fact]
        public void ShouldInsertAuditMessageAndHeadersIntoRepository()
        {
            // Arrange
            var handler = new AuditMessageHandler(_mockRepository.Object, _mockContext.Object);

            // Act
            handler.Execute("TestMessage", _headers);

            // Assert
            _mockRepository.Verify(x => x.InsertAudit(It.Is<Audit>(m =>
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
                m.ConsumerType == "RabbitMQ" &&
                m.Language == "C#"
            )), Times.Once());
        }
    }
}