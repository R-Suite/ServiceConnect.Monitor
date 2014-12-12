using System;
using System.Collections.Generic;
using MongoDB.Bson;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Interfaces
{
    public interface IErrorRepository
    {
        void InsertError(Error model);
        void EnsureIndex();
        IList<Error> Find(DateTime @from, DateTime to);
        Error Get(ObjectId id);
        void Remove(DateTime before);
        IList<Error> Find(Guid correlationId);
    }
}