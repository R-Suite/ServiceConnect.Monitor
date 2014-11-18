using System;
using System.Collections.Generic;
using Moq;
using Newtonsoft.Json;
using R.MessageBus.Monitor.Handlers;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;
using Xunit;

namespace R.MessageBus.Monitor.UnitTests.Handlers
{
    public class HearbeatHandlerTests
    {
        private readonly Mock<IHeartbeatRepository> _mockRepository;
        private readonly Dictionary<string, string> _headers;
        private readonly Heartbeat _message;

        public HearbeatHandlerTests()
        {
            _mockRepository = new Mock<IHeartbeatRepository>();
            _mockRepository.Setup(x => x.InsertHeartbeat(It.IsAny<Heartbeat>()));
            _headers = new Dictionary<string, string>();
            _message = new Heartbeat
            {
                ConsumerType = "RabbitMQ",
                CpuUsage = 50,
                Language = "C#",
                Location = "TestLocation",
                MemoryUsage = 500,
                ServiceName = "TestService",
                Timestamp = DateTime.Today
            };
        }

        [Fact]
        public void ShouldInsertAuditMessageAndHeadersIntoRepository()
        {
            // Arrange
            var handler = new HearbeatMessageHandler(_mockRepository.Object);

            // Act
            handler.Execute(JsonConvert.SerializeObject(_message), _headers);

            // Assert
            _mockRepository.Verify(x => x.InsertHeartbeat(It.Is<Heartbeat>(m =>
                m.ConsumerType == _message.ConsumerType &&
                m.Language == _message.Language &&
                m.Location == _message.Location &&
                m.ServiceName == _message.ServiceName &&
                m.CpuUsage == _message.CpuUsage &&
                m.MemoryUsage == _message.MemoryUsage &&
                m.Timestamp == _message.Timestamp
            )), Times.Once());
        }
    }
}