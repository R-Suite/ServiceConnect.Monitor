using System;
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
    public class AuditRepository : IAuditRepository
    {
        private readonly MongoCollection<Audit> _auditCollection;
        private readonly MongoCollection<Service> _serviceCollection;
        private readonly MongoCollection<ServiceMessage> _serviceMessagesCollection;

        public AuditRepository(string mongoConnectionString)
        {
            var mongoClient = new MongoClient(mongoConnectionString);
            MongoServer server = mongoClient.GetServer();
            var mongoDatabase = server.GetDatabase("RMessageBusMonitor");
            _auditCollection = mongoDatabase.GetCollection<Audit>("Audit");
            _serviceCollection = mongoDatabase.GetCollection<Service>("Services");
            _serviceMessagesCollection = mongoDatabase.GetCollection<ServiceMessage>("ServiceMessages");
        }

        public void EnsureIndex()
        {
            _auditCollection.CreateIndex(IndexKeys<Audit>.Descending(x => x.TimeSent));
            _auditCollection.CreateIndex(IndexKeys<Audit>.Ascending(x => x.CorrelationId));
        }

        public Audit Get(ObjectId objectId)
        {
            return _auditCollection.FindOneById(objectId);
        }

        public void Remove(DateTime before)
        {
            _auditCollection.Remove(Query<Audit>.LT(x => x.TimeSent, before));
        }

        public IList<Audit> Find(Guid correlationId)
        {
            return _auditCollection.Find(Query<Audit>.EQ(x => x.CorrelationId, correlationId)).OrderByDescending(x => x.TimeSent).ToList();
        }

        public void InsertAudit(Audit model)
        {
            _auditCollection.Insert(model);

            _serviceCollection.Update(

                Query.And(
                    Query<Service>.EQ(x => x.Name, model.SourceAddress),
                    Query<Service>.EQ(x => x.InstanceLocation, model.SourceMachine)
                ),
                Update<Service>.AddToSet(x => x.Out, model.TypeName)
                               .Set(x => x.Language, model.Language)
                               .Set(x => x.ConsumerType, model.ConsumerType),
                UpdateFlags.Upsert);

            _serviceCollection.Update(
                Query.And(
                    Query<Service>.EQ(x => x.Name, model.DestinationAddress),
                    Query<Service>.EQ(x => x.InstanceLocation, model.DestinationMachine)
                ),
                Update<Service>.AddToSet(x => x.In, model.TypeName)
                               .Set(x => x.Language, model.Language)
                               .Set(x => x.ConsumerType, model.ConsumerType),
                UpdateFlags.Upsert);

            _serviceMessagesCollection.Update(
                Query.And(
                    Query<ServiceMessage>.EQ(x => x.In, model.DestinationAddress),
                    Query<ServiceMessage>.EQ(x => x.Out, model.SourceAddress)
                ),
                Update<ServiceMessage>.Inc(x => x.Count, 1).
                                       Set(x => x.LastSent, model.TimeReceived).
                                       Set(x => x.Type, model.TypeName),
                UpdateFlags.Upsert);
        }

        public IList<Audit> Find(DateTime @from, DateTime to)
        {
            var result = _auditCollection.AsQueryable().Where(x =>
               x.TimeSent >= from &&
               x.TimeSent <= to).OrderByDescending(x => x.TimeSent);

            return result.ToList();
        }
    }
}