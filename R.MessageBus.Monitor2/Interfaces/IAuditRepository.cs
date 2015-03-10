using System;
using System.Collections.Generic;
using MongoDB.Bson;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Interfaces
{
    public interface IAuditRepository
    {
        void InsertAudit(Audit model);
        IList<Audit> Find(DateTime @from, DateTime to);
        void EnsureIndex();
        Audit Get(ObjectId objectId);
        void Remove(DateTime before);
        IList<Audit> Find(Guid correlationId);
    }
}