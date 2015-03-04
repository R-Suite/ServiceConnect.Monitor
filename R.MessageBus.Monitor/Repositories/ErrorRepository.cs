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
    public class ErrorRepository : IErrorRepository
    {
        private readonly MongoCollection<Error> _errorCollection;

        public ErrorRepository(string mongoConnectionString)
        {
            var mongoClient = new MongoClient(mongoConnectionString);
            MongoServer server = mongoClient.GetServer();
            var mongoDatabase = server.GetDatabase("RMessageBusMonitor");
            _errorCollection = mongoDatabase.GetCollection<Error>("Error");
        }

        public void InsertError(Error model)
        {
            _errorCollection.Insert(model);
        }
        public void EnsureIndex()
        {
            _errorCollection.CreateIndex(IndexKeys<Error>.Ascending(x => x.TimeSent));
        }

        public IList<Error> Find(DateTime @from, DateTime to)
        {
            var result = _errorCollection.AsQueryable().Where(x =>
               x.TimeSent >= from &&
               x.TimeSent <= to).OrderByDescending(x => x.TimeSent);

            return result.ToList();
        }

        public Error Get(Guid id)
        {        	
            return _errorCollection.FindOneById(id);
        }

        public void Remove(DateTime before)
        {
            _errorCollection.Remove(Query<Error>.LT(x => x.TimeSent, before));
        }

        public void Remove(Guid id)
        {
            _errorCollection.Remove(Query<Error>.EQ(x => x.Id, id));
        }

        public IList<Error> Find(Guid correlationId)
        {
            return _errorCollection.Find(Query<Error>.EQ(x => x.CorrelationId, correlationId)).ToList();
        }
    }
}