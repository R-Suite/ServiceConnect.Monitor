using System.Collections.Generic;
using MongoDB.Bson;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Interfaces
{
    public interface IServiceRepository
    {
        IList<Service> Find();
        Service Find(string name, string location);
        IList<Service> FindByName(string name);
        void Update(Service model);
        Service Get(ObjectId id);
        void Delete(ObjectId id);
        void EnsureIndex();
    }
}
