using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using R.MessageBus.Monitor.Controllers;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;
using Xunit;

namespace R.MessageBus.Monitor.UnitTests.Controllers
{
    public class HeartbeatControllerTests
    {
        private readonly Mock<IHeartbeatRepository> _mockRepository;

        public HeartbeatControllerTests()
        {
            _mockRepository = new Mock<IHeartbeatRepository>();
            var models = new List<Heartbeat>
            {
                new Heartbeat
                {
                    ServiceName = "Service1",
                    Location = "TestLocation",
                    Timestamp = DateTime.Today.AddDays(-1)
                },
                new Heartbeat
                {
                    ServiceName = "Service1",
                    Location = "TestLocation",
                    Timestamp = DateTime.Today
                }
            };
            _mockRepository.Setup(x => x.Find(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new QueryResult<Heartbeat>
            {
                Results = models,
                Count = 2
            });
        }

        [Fact]
        public void ShouldFindAllServiceMessages()
        {
            // Arrange
            var handler = new HeartbeatController(_mockRepository.Object);

            // Act
            var results = handler.Find("Service1", "TestLocation", DateTime.Today.AddDays(-1), DateTime.Today, 100, 2);

            // Assert
            Assert.Equal(2, results.Count);
            _mockRepository.Verify(x => x.Find("Service1", "TestLocation", DateTime.Today.AddDays(-1), DateTime.Today, 100, 2), Times.Once);
            Assert.NotNull(results.Results.FirstOrDefault(x => x.ServiceName == "Service1" && x.Location == "TestLocation" && x.Timestamp == DateTime.Today));
            Assert.NotNull(results.Results.FirstOrDefault(x => x.ServiceName == "Service1" && x.Location == "TestLocation" && x.Timestamp == DateTime.Today.AddDays(-1)));
        }
    }
}