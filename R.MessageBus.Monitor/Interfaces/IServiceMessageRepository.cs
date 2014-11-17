using System.Collections.Generic;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Interfaces
{
    public interface IServiceMessageRepository
    {
        IList<ServiceMessage> Find();
    }
}
