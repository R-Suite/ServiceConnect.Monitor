using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly MongoCollection<Tag> _tagCollection;

        public TagRepository(string mongoConnectionString)
        {
            var mongoClient = new MongoClient(mongoConnectionString);
            MongoServer server = mongoClient.GetServer();
            var mongoDatabase = server.GetDatabase("RMessageBusMonitor");
            _tagCollection = mongoDatabase.GetCollection<Tag>("Tags");
        }

        public List<Tag> Find()
        {
            return _tagCollection.AsQueryable().ToList();
        }

        public void Insert(string tag)
        {
            var model = new Tag
            {
                Name = tag,
                Id = Guid.NewGuid()
            };
            _tagCollection.Insert(model);
        }
    }
}