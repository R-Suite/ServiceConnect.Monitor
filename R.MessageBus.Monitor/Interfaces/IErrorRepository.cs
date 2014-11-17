using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Interfaces
{
    public interface IErrorRepository
    {
        void InsertError(Error model);
    }
}