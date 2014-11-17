using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Interfaces
{
    public interface IAuditRepository
    {
        void InsertAudit(Audit model);
    }
}