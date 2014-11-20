using MongoDB.Driver;
using MongoDB.Driver.Builders;
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
    }
}