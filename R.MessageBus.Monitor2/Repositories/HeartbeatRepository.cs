using System;
using System.Collections.Generic;
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
                Query.And(
                    Query<Service>.EQ(x => x.Name, model.Name),
                    Query<Service>.EQ(x => x.InstanceLocation, model.Location)
                ),
                Update<Service>.Set(x => x.Language, model.Language).
                                Set(x => x.ConsumerType, model.ConsumerType).
                                Set(x => x.LastHeartbeat, model.Timestamp).
                                Set(x => x.LatestCpu, model.LatestCpu).
                                Set(x => x.LatestMemory, model.LatestMemory),
                UpdateFlags.Upsert);
        }

        public List<Heartbeat> Find(string name, string location, DateTime @from, DateTime to)
        {
            var result = _heartbeatsCollection.AsQueryable().Where(x => 
                x.Timestamp >= from &&
                x.Timestamp <= to && 
                x.Name == name &&
                x.Location == location).OrderByDescending(x => x.Timestamp);

            return result.ToList();
        }

        public void Remove(string name, string location)
        {
            _heartbeatsCollection.Remove(
                Query.And(
                    Query<Heartbeat>.EQ(x => x.Name, name),
                    Query<Heartbeat>.EQ(x => x.Location, location)
                )
            );
        }

        public void EnsureIndex()
        {
            _heartbeatsCollection.CreateIndex(IndexKeys<Heartbeat>.Ascending(x => x.Name).Ascending(x => x.Location).Descending(x => x.Timestamp));
            _heartbeatsCollection.CreateIndex(IndexKeys<Heartbeat>.Ascending(x => x.Name).Ascending(x => x.Location));
        }

        public void Remove(DateTime before)
        {
            _heartbeatsCollection.Remove(Query<Heartbeat>.LT(x => x.Timestamp, before));
        }
    }
}