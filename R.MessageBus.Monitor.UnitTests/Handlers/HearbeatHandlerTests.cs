using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
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
        private Mock<IHubContext> _mockContext;

        public HearbeatHandlerTests()
        {
            _mockRepository = new Mock<IHeartbeatRepository>();
            _mockRepository.Setup(x => x.InsertHeartbeat(It.IsAny<Heartbeat>()));
            _mockContext = new Mock<IHubContext>();

            _headers = new Dictionary<string, string>();
            _message = new Heartbeat
            {
                ConsumerType = "RabbitMQ",
                LatestCpu = 50,
                Language = "C#",
                Location = "TestLocation",
                LatestMemory = 500,
                Name = "TestService",
                Timestamp = DateTime.Today
            };
        }

        [Fact]
        public void ShouldInsertAuditMessageAndHeadersIntoRepository()
        {
            // Arrange
            var handler = new HearbeatMessageHandler(_mockRepository.Object, _mockContext.Object);

            // Act
            handler.Execute(JsonConvert.SerializeObject(_message), _headers);

            // Assert
            _mockRepository.Verify(x => x.InsertHeartbeat(It.Is<Heartbeat>(m =>
                m.ConsumerType == _message.ConsumerType &&
                m.Language == _message.Language &&
                m.Location == _message.Location &&
                m.Name == _message.Name &&
                m.LatestCpu == _message.LatestCpu &&
                m.LatestMemory == _message.LatestMemory &&
                m.Timestamp == _message.Timestamp
            )), Times.Once());
        }
    }
}