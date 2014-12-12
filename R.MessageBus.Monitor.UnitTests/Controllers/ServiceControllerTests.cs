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
    public class ServiceControllerTests
    {
        private readonly Mock<IServiceRepository> _mockRepository;
        private readonly Mock<ITagRepository> _mockTagRepository;
        private readonly Mock<IHeartbeatRepository> _mockHeartbeatRepository;

        public ServiceControllerTests()
        {
            _mockRepository = new Mock<IServiceRepository>();
            _mockTagRepository = new Mock<ITagRepository>();
            var models = new List<Service>
            {
                new Service
                {
                    Name = "Service1",
                    In = new List<string> { "Message1" },
                    Out = new List<string> { "Message2" },
                    InstanceLocation = "Loc1",
                    LastHeartbeat = DateTime.Today.AddDays(-1)
                },
                new Service
                {
                    Name = "Service2",
                    In = new List<string> { "Message2" },
                    Out = new List<string> { "Message1" }
                },
                new Service
                {
                    Name = "Service1",
                    In = new List<string> { "Message1" },
                    Out = new List<string> { "Message2" },
                    InstanceLocation = "Loc2",
                    LastHeartbeat = DateTime.Today
                }
            };

            _mockRepository.Setup(x => x.Find()).Returns(models);

            var serviceModels = new List<Service>
            {
                new Service
                {
                    Name = "Service1",
                    In = new List<string> { "Message1" },
                    Out = new List<string> { "Message2" },
                    InstanceLocation = "Loc1",
                    LastHeartbeat = DateTime.Today.AddDays(-1)
                },
                new Service
                {
                    Name = "Service1",
                    In = new List<string> { "Message1" },
                    Out = new List<string> { "Message2" },
                    InstanceLocation = "Loc2",
                    LastHeartbeat = DateTime.Today
                }
            };

            _mockRepository.Setup(x => x.FindByName("Service1")).Returns(serviceModels);
        }

        [Fact]
        public void ShouldFindAllServices()
        {
            // Arrange
            var handler = new ServiceController(_mockRepository.Object, _mockTagRepository.Object, _mockHeartbeatRepository.Object);

            // Act
            var results = handler.FindServices(null);

            // Assert
            var service1 = results.FirstOrDefault(x => x.Name == "Service1" && x.In.Contains("Message1") && x.Out.Contains("Message2") && x.InstanceLocation == "Loc1");
            var service2 = results.FirstOrDefault(x => x.Name == "Service1" && x.In.Contains("Message1") && x.Out.Contains("Message2") && x.InstanceLocation == "Loc2");
            Assert.NotNull(service1);
            Assert.NotNull(service2);
        }

        [Fact]
        public void ShouldFindAllEndpoints()
        {
            // Arrange
            var handler = new ServiceController(_mockRepository.Object, _mockTagRepository.Object, _mockHeartbeatRepository.Object);

            // Act
            IList<Service> results = handler.FindEndpoints(null);

            // Assert
            var service1 = results.FirstOrDefault(x => x.Name == "Service1" && x.In.Contains("Message1") && x.Out.Contains("Message2") && x.LastHeartbeat == DateTime.Today.AddDays(-1));
            var service2 = results.FirstOrDefault(x => x.Name == "Service2" && x.In.Contains("Message2") && x.Out.Contains("Message1"));
            Assert.NotNull(service1);
            Assert.NotNull(service2);
            Assert.Equal(1, results.Count(x => x.Name == "Service2" && x.In.Contains("Message2")));
        }
    }
}