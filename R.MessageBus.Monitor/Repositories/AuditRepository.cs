//Copyright (C) 2015  Timothy Watson, Jakub Pachansky

//This program is free software; you can redistribute it and/or
//modify it under the terms of the GNU General Public License
//as published by the Free Software Foundation; either version 2
//of the License, or (at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Repositories
{
    public class AuditRepository : IAuditRepository
    {
        private readonly MongoCollection<Audit> _auditCollection;
        private readonly MongoCollection<ServiceMessage> _serviceMessagesCollection;

        public AuditRepository(string mongoConnectionString)
        {
            var mongoClient = new MongoClient(mongoConnectionString);
            MongoServer server = mongoClient.GetServer();
            var mongoDatabase = server.GetDatabase("RMessageBusMonitor");
            _auditCollection = mongoDatabase.GetCollection<Audit>("Audit");
            _serviceMessagesCollection = mongoDatabase.GetCollection<ServiceMessage>("ServiceMessages");
        }

        public void EnsureIndex()
        {
            _auditCollection.CreateIndex(IndexKeys<Audit>.Descending(x => x.TimeSent));
            _auditCollection.CreateIndex(IndexKeys<Audit>.Ascending(x => x.CorrelationId));
        }

        public Audit Get(ObjectId objectId)
        {
            return _auditCollection.FindOneById(objectId);
        }

        public void Remove(DateTime before)
        {
            _auditCollection.Remove(Query<Audit>.LT(x => x.TimeSent, before));
        }

        public IList<Audit> Find(Guid correlationId)
        {
            return _auditCollection.Find(Query<Audit>.EQ(x => x.CorrelationId, correlationId)).OrderByDescending(x => x.TimeSent).ToList();
        }

        public void InsertAudit(Audit model)
        {
            _auditCollection.Insert(model);

            _serviceMessagesCollection.Update(
                Query.And(
                    Query<ServiceMessage>.EQ(x => x.In, model.DestinationAddress),
                    Query<ServiceMessage>.EQ(x => x.Out, model.SourceAddress)
                ),
                Update<ServiceMessage>.Inc(x => x.Count, 1).
                                       Set(x => x.LastSent, model.TimeReceived).
                                       Set(x => x.Type, model.TypeName),
                UpdateFlags.Upsert);
        }

        public IList<Audit> Find(DateTime @from, DateTime to)
        {
            var result = _auditCollection.AsQueryable().Where(x =>
               x.TimeSent >= from &&
               x.TimeSent <= to).OrderByDescending(x => x.TimeSent);

            return result.ToList();
        }
    }
}