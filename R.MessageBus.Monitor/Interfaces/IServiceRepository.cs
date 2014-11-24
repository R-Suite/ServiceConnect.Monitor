using System.Collections.Generic;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Interfaces
{
    public interface IServiceRepository
    {
        IList<Service> Find();
        Service Find(string name, string location);
        IList<Service> FindByName(string name);
        void Update(Service model);
    }
}
