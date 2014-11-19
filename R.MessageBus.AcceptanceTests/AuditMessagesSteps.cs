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
    public class AuditMessagesSteps
    {
        private readonly MongoCollection<Audit> _auditCollection;
        private static IBus _bus;
        private MongoCursor<Audit> _audits;
        private Guid _correlationId;
        private readonly MongoCollection<Service> _serviceCollection;
        private readonly MongoCollection<ServiceMessage> _serviceMessageCollection;

        public AuditMessagesSteps()
        {
            var mongoClient = new MongoClient("mongodb://lonappdev04/");
            MongoServer server = mongoClient.GetServer();
            var mongoDatabase = server.GetDatabase("RMessageBusMonitor");
            _auditCollection = mongoDatabase.GetCollection<Audit>("Audit");
            _serviceCollection = mongoDatabase.GetCollection<Service>("Services");
            _serviceMessageCollection = mongoDatabase.GetCollection<ServiceMessage>("ServiceMessages");
            _auditCollection.RemoveAll();
            _serviceCollection.RemoveAll();
            _serviceMessageCollection.RemoveAll();

            _bus = Bus.Initialize(config =>
            {
                config.TransportSettings.Host = "lonappdev04";
                config.SetAuditingEnabled(true);
                config.SetQueueName("R.MessageBus.Monitor.AcceptanceTests");
                config.AddQueueMapping(typeof(TestMessage), "R.MessageBus.Monitor.AcceptanceTests");
                config.ScanForMesssageHandlers = true;
            });
            _bus.StartConsuming();
        }

        [Given(@"I receive audit messages")]
        public void GivenIReceiveAuditMessages()
        {
            _correlationId = Guid.NewGuid();

            _bus.Send(new TestMessage(_correlationId)
            {
                Name = "Test Username"
            });
        }
        
        [When(@"I query the audit database collection")]
        public void WhenIQueryTheAuditDatabaseCollection()
        {
            for (int i = 0; i < 2000; i++)
            {
                _audits = _auditCollection.FindAll();
                if (_audits.Any())
                {
                    break;
                }
                Thread.Sleep(10);
            }
        }
        
        [Then(@"the audit messages should exist in the database")]
        public void ThenTheAuditMessagesShouldExistInTheDatabase()
        {
            Audit audit = _auditCollection.FindAll().First();
            Assert.Equal("R.MessageBus.AcceptanceTests.TestMessage, R.MessageBus.AcceptanceTests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", audit.FullTypeName);
            var message = JsonConvert.DeserializeObject<TestMessage>(audit.Body);
            Assert.Equal("Test Username", message.Name);
            Assert.Equal(_correlationId, message.CorrelationId);
        }

        [When(@"I query the service database collection")]
        public void WhenIQueryTheServiceDatabaseCollection()
        {
            for (int i = 0; i < 2000; i++)
            {
                var services = _serviceCollection.FindAll();
                if (services.Any())
                {
                    break;
                }
                Thread.Sleep(10);
            }
        }

        [Then(@"the destination service should exist in the database")]
        public void ThenTheDestinationServiceShouldExistInTheDatabase()
        {
            var destination = _serviceCollection.AsQueryable().FirstOrDefault(x => x.Name == "R.MessageBus.Monitor.AcceptanceTests" && x.In.Contains("TestMessage"));
            Assert.NotNull(destination);
        }

        [Then(@"the source service should exist in the database")]
        public void ThenTheSourceServiceShouldExistInTheDatabase()
        {
            var source = _serviceCollection.AsQueryable().FirstOrDefault(x => x.Name == "R.MessageBus.Monitor.AcceptanceTests" && x.Out.Contains("TestMessage"));
            Assert.NotNull(source);
        }

        [When(@"I query the service message database collection")]
        public void WhenIQueryTheServiceMessageDatabaseCollection()
        {
            for (int i = 0; i < 2000; i++)
            {
                var messages = _serviceMessageCollection.FindAll();
                if (messages.Any())
                {
                    break;
                }
                Thread.Sleep(10);
            }
        }

        [Then(@"the message type connecting destination and source should exist in the database")]
        public void ThenTheMessageTypeShouldExistInTheDatabse()
        {
            var message = _serviceMessageCollection.AsQueryable().FirstOrDefault(x => x.Type == "TestMessage" &&
                                                                                      x.In == "R.MessageBus.Monitor.AcceptanceTests" &&
                                                                                      x.Out == "R.MessageBus.Monitor.AcceptanceTests");
            Assert.NotNull(message);
        }

        [AfterTestRun]
        public static void After()
        {
            _bus.Dispose();
        }
    }
}
