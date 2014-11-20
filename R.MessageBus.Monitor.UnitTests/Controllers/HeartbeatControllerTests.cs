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
                    Name = "Service1",
                    Location = "TestLocation",
                    Timestamp = DateTime.Today.AddDays(-1)
                },
                new Heartbeat
                {
                    Name = "Service1",
                    Location = "TestLocation",
                    Timestamp = DateTime.Today
                }
            };
            _mockRepository.Setup(x => x.Find(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(models);
        }

        [Fact]
        public void ShouldFindAllServiceMessages()
        {
            // Arrange
            var handler = new HeartbeatController(_mockRepository.Object);

            // Act
            var results = handler.Find("Service1", "TestLocation", DateTime.Today.AddDays(-1), DateTime.Today);

            // Assert
            Assert.Equal(2, results.Count);
            _mockRepository.Verify(x => x.Find("Service1", "TestLocation", DateTime.Today.AddDays(-1), DateTime.Today), Times.Once);
            Assert.NotNull(results.FirstOrDefault(x => x.Name == "Service1" && x.Location == "TestLocation" && x.Timestamp == DateTime.Today));
            Assert.NotNull(results.FirstOrDefault(x => x.Name == "Service1" && x.Location == "TestLocation" && x.Timestamp == DateTime.Today.AddDays(-1)));
        }
    }
}