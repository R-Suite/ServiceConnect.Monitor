using System.Collections.Generic;
using System.Linq;
using Moq;
using R.MessageBus.Monitor.Controllers;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;
using Xunit;

namespace R.MessageBus.Monitor.UnitTests.Controllers
{
    public class ServiceMessageControllerTests
    {
        private readonly Mock<IServiceMessageRepository> _mockRepository;

        public ServiceMessageControllerTests()
        {
            _mockRepository = new Mock<IServiceMessageRepository>();
            var models = new List<ServiceMessage>
            {
                new ServiceMessage
                {
                    Count = 1,
                    Type = "Message1",
                    In = "Service1",
                    Out = "Service2"
                },
                new ServiceMessage
                {
                    Count = 2,
                    Type = "Message2",
                    In = "Service2",
                    Out = "Service1"
                }
            };
            _mockRepository.Setup(x => x.Find()).Returns(models);
        }

        [Fact]
        public void ShouldFindAllServiceMessages()
        {
            // Arrange
            var handler = new ServiceMessageController(_mockRepository.Object);

            // Act
            var results = handler.FindServices();

            // Assert
            var message1 = results.FirstOrDefault(x => x.Type == "Message1" && x.In == "Service1" && x.Out == "Service2");
            var message2 = results.FirstOrDefault(x => x.Type == "Message2" && x.In == "Service2" && x.Out == "Service1");
            Assert.NotNull(message1);
            Assert.NotNull(message2);
        }
    }
}