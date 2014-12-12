using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
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

        public void EnsureIndex()
        {
            _serviceCollection.CreateIndex(IndexKeys<Service>.Ascending(x => x.Name).Ascending(x => x.InstanceLocation));
            _serviceCollection.CreateIndex(IndexKeys<Service>.Ascending(x => x.Name));
            _serviceCollection.CreateIndex(IndexKeys<Service>.Ascending(x => x.Tags));
        }

        public IList<Service> Find()
        {
            return _serviceCollection.AsQueryable().ToList();
        }

        public Service Find(string name, string location)
        {
            return _serviceCollection.AsQueryable().FirstOrDefault(x => x.Name == name && x.InstanceLocation == location);
        }

        public IList<Service> FindByName(string name)
        {
            return _serviceCollection.AsQueryable().Where(x => x.Name == name).ToList();
        }

        public void Update(Service model)
        {
            _serviceCollection.Save(model);
        }

        public Service Get(ObjectId id)
        {
            return _serviceCollection.FindOneById(id);
        }

        public void Delete(ObjectId id)
        {
            _serviceCollection.Remove(Query<Service>.EQ(x => x.Id, id));
        }
    }
}