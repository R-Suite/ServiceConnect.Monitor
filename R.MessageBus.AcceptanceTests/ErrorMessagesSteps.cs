using System;
using System.Linq;
using System.Threading;
using MongoDB.Driver;
using Newtonsoft.Json;
using R.MessageBus.Interfaces;
using R.MessageBus.Monitor.Models;
using TechTalk.SpecFlow;
using Xunit;

namespace R.MessageBus.AcceptanceTests
{
    [Binding]
    public class ErrorMessagesSteps
    {
        private readonly MongoCollection<Error> _errorCollection;
        private static IBus _bus;
        private MongoCursor<Error> _errors;
        private Guid _correlationId;

        public ErrorMessagesSteps()
        {
            var mongoClient = new MongoClient("mongodb://localhost:37017/");
            MongoServer server = mongoClient.GetServer();
            var mongoDatabase = server.GetDatabase("RMessageBusMonitor");
            _errorCollection = mongoDatabase.GetCollection<Error>("Errors");

            _errorCollection.RemoveAll();

            _bus = Bus.Initialize(config =>
            {
                config.TransportSettings.Host = "localhost";
                config.SetAuditingEnabled(true);
                config.SetQueueName("R.MessageBus.Monitor.AcceptanceTests");
                config.AddQueueMapping(typeof(TestErrorMessage), "R.MessageBus.Monitor.AcceptanceTests");
                config.ScanForMesssageHandlers = true;
                config.TransportSettings.Username = "guest";
                config.TransportSettings.Password = "guest";
            });
            _bus.StartConsuming();
        }

        [Given(@"I receive error messages")]
        public void GivenIReceiveErrorMessages()
        {
            _correlationId = Guid.NewGuid();

            _bus.Send(new TestErrorMessage(_correlationId)
            {
                Name = "Test Username"
            });
        }
        
        [When(@"I query the error database collection")]
        public void WhenIQueryTheErrorDatabaseCollection()
        {
            for (int i = 0; i < 2000; i++)
            {
                _errors = _errorCollection.FindAll();
                if (_errors.Any())
                {
                    break;
                }
                Thread.Sleep(10);
            }
        }
        
        [Then(@"the error messages should exist in the database")]
        public void ThenTheErrorMessagesShouldExistInTheDatabase()
        {
            Error error = _errorCollection.FindAll().First();
            Assert.Equal("R.MessageBus.AcceptanceTests.TestMessage, R.MessageBus.AcceptanceTests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", error.FullTypeName);
            var message = JsonConvert.DeserializeObject<TestMessage>(error.Body);
            Assert.Equal("Test Username", message.Name);
            Assert.Equal(_correlationId, message.CorrelationId);
        }
    }
}
