using System;
using System.Linq;
using System.Threading;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using R.MessageBus.Interfaces;
using R.MessageBus.Monitor.Models;
using TechTalk.SpecFlow;
using Xunit;

namespace R.MessageBus.AcceptanceTests
{
    [Binding]
    public class HeartbeatMessagesSteps
    {
        private readonly MongoCollection<Heartbeat> _heartbeatCollection;

        private static IBus _bus;
        private MongoCursor<Heartbeat> _heartbeats;
        private Guid _correlationId;
        private readonly MongoCollection<Service> _serviceCollection;

        public HeartbeatMessagesSteps()
        {
            var mongoClient = new MongoClient("mongodb://lonappdev04");
            MongoServer server = mongoClient.GetServer();
            var mongoDatabase = server.GetDatabase("RMessageBusMonitor");
            _heartbeatCollection = mongoDatabase.GetCollection<Heartbeat>("Errors");
            _serviceCollection = mongoDatabase.GetCollection<Service>("Services");

            _heartbeatCollection.RemoveAll();
            _serviceCollection.RemoveAll();

            _bus = Bus.Initialize(config =>
            {
                config.TransportSettings.Host = "lonappdev04";
                config.SetAuditingEnabled(true);
                config.SetQueueName("R.MessageBus.Monitor.AcceptanceTests");
                config.AddQueueMapping(typeof(TestMessage), "R.MessageBus.Monitor.AcceptanceTests");
                config.ScanForMesssageHandlers = true;
                config.TransportSettings.Username = "guest";
                config.TransportSettings.Password = "guest";
            });
            _bus.StartConsuming();
        }


        [Given(@"I receive heartbeat messages")]
        public void GivenIReceiveHeartbeatMessages()
        {
            _bus.Send(new TestMessage(_correlationId)
            {
                Name = "Test Username"
            });

            Thread.Sleep(1000);

            _correlationId = Guid.NewGuid();

            _bus.Send(new TestMessage(_correlationId)
            {
                Name = "Test Username 2"
            });
        }
        
        [When(@"I query the heartbeat database collection")]
        public void WhenIQueryTheHeartbeatDatabaseCollection()
        {
            for (int i = 0; i < 2000; i++)
            {
                _heartbeats = _heartbeatCollection.FindAll();
                if (_heartbeats.Count() >= 2)
                {
                    break;
                }
                Thread.Sleep(10);
            }
        }
        
        [Then(@"the heartbeat messages should exist in the database")]
        public void ThenTheHeartbeatMessagesShouldExistInTheDatabase()
        {
            Heartbeat heartbeat = _heartbeatCollection.FindAll().First();
            Assert.Equal("R.MessageBus.Monitor.AcceptanceTests", heartbeat.Name);
            Assert.Equal("C#", heartbeat.Language);
            Assert.Equal("RabbitMQ", heartbeat.ConsumerType);
        }
        
        [Then(@"the service should exist in the database")]
        public void ThenTheServiceShouldExistInTheDatabase()
        {
            var service = _serviceCollection.AsQueryable().FirstOrDefault(x => x.Name == "R.MessageBus.Monitor.AcceptanceTests" && x.In.Contains("TestMessage"));
            Assert.NotNull(service);
        }
    }
}
