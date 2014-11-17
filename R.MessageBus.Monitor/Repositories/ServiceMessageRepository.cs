using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Repositories
{
    public class ServiceMessageRepository : IServiceMessageRepository
    {
        private readonly MongoCollection<ServiceMessage> _serviceMessagesCollection;

        public ServiceMessageRepository(string mongoConnectionString)
        {
            var mongoClient = new MongoClient(mongoConnectionString);
            MongoServer server = mongoClient.GetServer();
            var mongoDatabase = server.GetDatabase("RMessageBusMonitor");
            _serviceMessagesCollection = mongoDatabase.GetCollection<ServiceMessage>("ServiceMessages");
        }

        public IList<ServiceMessage> Find()
        {
            return _serviceMessagesCollection.AsQueryable().ToList();
        }
    }
}