using System.Collections.Generic;
using System.Linq;
using Moq;
using R.MessageBus.Monitor.Controllers;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;
using Xunit;

namespace R.MessageBus.Monitor.UnitTests.Controllers
{
    public class ServiceControllerTests
    {
        private readonly Mock<IServiceRepository> _mockRepository;

        public ServiceControllerTests()
        {
            _mockRepository = new Mock<IServiceRepository>();
            var models = new List<Service>
            {
                new Service
                {
                    Name = "Service1",
                    In = new List<string> { "Message1" },
                    Out = new List<string> { "Message2" }
                },
                new Service
                {
                    Name = "Service2",
                    In = new List<string> { "Message2" },
                    Out = new List<string> { "Message1" }
                }
            };
            _mockRepository.Setup(x => x.Find()).Returns(models);
        }

        [Fact]
        public void ShouldFindAllServiceMessages()
        {
            // Arrange
            var handler = new ServiceController(_mockRepository.Object);

            // Act
            var results = handler.FindServices();

            // Assert
            var service1 = results.FirstOrDefault(x => x.Name == "Service1" && x.In.Contains("Message1") && x.Out.Contains("Message2"));
            var service2 = results.FirstOrDefault(x => x.Name == "Service2" && x.In.Contains("Message2") && x.Out.Contains("Message1"));
            Assert.NotNull(service1);
            Assert.NotNull(service2);
        }
    }
}