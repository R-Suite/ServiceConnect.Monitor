using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly MongoCollection<Service> _serviceCollection;

        public ServiceRepository(string mongoConnectionString)
        {
            var mongoClient = new MongoClient(mongoConnectionString);
            MongoServer server = mongoClient.GetServer();
            var mongoDatabase = server.GetDatabase("RMessageBusMonitor");
            _serviceCollection = mongoDatabase.GetCollection<Service>("Services");
        }

        public IList<Service> Find()
        {
            return _serviceCollection.AsQueryable().ToList();
        }

        public IList<Service> FindByName(string name)
        {
            return _serviceCollection.AsQueryable().Where(x => x.Name == name).ToList();
        }
    }
}