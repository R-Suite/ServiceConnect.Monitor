using System;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Repositories
{
    public class HeartbeatRepository : IHeartbeatRepository
    {
        private readonly MongoCollection<Heartbeat> _heartbeatsCollection;
        private readonly MongoCollection<Service> _serviceCollection;

        public HeartbeatRepository(string mongoConnectionString)
        {
            var mongoClient = new MongoClient(mongoConnectionString);
            MongoServer server = mongoClient.GetServer();
            var mongoDatabase = server.GetDatabase("RMessageBusMonitor");
            _heartbeatsCollection = mongoDatabase.GetCollection<Heartbeat>("ServiceHeartbeats");
            _serviceCollection = mongoDatabase.GetCollection<Service>("Services");
        }

        public void InsertHeartbeat(Heartbeat model)
        {
            _heartbeatsCollection.Insert(model);

            _serviceCollection.Update(
                Query<Service>.EQ(x => x.Name, model.ServiceName),
                Update<Service>.AddToSet(x => x.InstanceLocation, model.Location).
                                Set(x => x.Language, model.Language).
                                Set(x => x.ConsumerType, model.ConsumerType).
                                Set(x => x.LastHeartbeat, model.Timestamp),
                UpdateFlags.Upsert);
        }

        public QueryResult<Heartbeat> Find(string endPoint, string location, DateTime @from, DateTime to, int pageSize, int pageNumber)
        {
            var result = _heartbeatsCollection.AsQueryable().Where(x => 
                x.Timestamp >= from &&
                x.Timestamp <= to && x.ServiceName == endPoint &&
                x.Location == location).Skip((pageNumber - 1) * pageSize);
            var count = _heartbeatsCollection.AsQueryable().Count(x =>
                x.Timestamp >= from &&
                x.Timestamp <= to && x.ServiceName == endPoint &&
                x.Location == location);
            return new QueryResult<Heartbeat>
            {
                Count = count,
                Results = result.ToList()
            };
        }
    }
}