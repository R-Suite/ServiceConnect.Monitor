using MongoDB.Driver;

namespace ServiceConnect.Monitor.Interfaces
{
    public interface IMongoRepository
    {
        MongoDatabase Database { get; }
    }
}