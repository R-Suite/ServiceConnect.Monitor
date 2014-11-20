using System;
using System.Collections.Generic;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Interfaces
{
    public interface IHeartbeatRepository
    {
        void InsertHeartbeat(Heartbeat heartbeat);
        List<Heartbeat> Find(string name, string location, DateTime @from, DateTime to);
    }
}