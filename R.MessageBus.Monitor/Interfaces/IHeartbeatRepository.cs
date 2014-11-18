using System;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Interfaces
{
    public interface IHeartbeatRepository
    {
        void InsertHeartbeat(Heartbeat heartbeat);
        QueryResult<Heartbeat> Find(string endPoint, string location, DateTime @from, DateTime to, int pageSize, int pageNumber);
    }
}