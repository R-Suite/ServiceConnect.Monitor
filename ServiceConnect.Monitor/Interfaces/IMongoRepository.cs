using MongoDB.Driver;

namespace ServiceConnect.Monitor.Interfaces
{
    public interface IMongoRepository
    {
        IMongoDatabase Database { get; }
    }
}