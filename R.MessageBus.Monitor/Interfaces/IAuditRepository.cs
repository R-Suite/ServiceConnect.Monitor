using System;
using System.Collections.Generic;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Interfaces
{
    public interface IAuditRepository
    {
        void InsertAudit(Audit model);
        IList<Audit> Find(DateTime @from, DateTime to);
    }
}