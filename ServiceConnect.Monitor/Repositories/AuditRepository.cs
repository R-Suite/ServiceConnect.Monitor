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
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using ServiceConnect.Monitor.Interfaces;
using ServiceConnect.Monitor.Models;

namespace ServiceConnect.Monitor.Repositories
{
    public class AuditRepository : IAuditRepository
    {
        private readonly IMongoCollection<Audit> _auditCollection;
        private readonly IMongoCollection<ServiceMessage> _serviceMessagesCollection;

        public AuditRepository(IMongoRepository mongoRepository, string auditCollecitonName, string serviceMessagesCollectionName)
        {
            _auditCollection = mongoRepository.Database.GetCollection<Audit>(auditCollecitonName);
            _serviceMessagesCollection = mongoRepository.Database.GetCollection<ServiceMessage>(serviceMessagesCollectionName);
        }

        public async Task EnsureIndex()
        {
            await _auditCollection.Indexes.CreateOneAsync(Builders<Audit>.IndexKeys.Descending(x => x.TimeSent));
            await _auditCollection.Indexes.CreateOneAsync(Builders<Audit>.IndexKeys.Ascending(x => x.CorrelationId));
        }

        public Task<Audit> Get(ObjectId objectId)
        {
            return _auditCollection.Find(Builders<Audit>.Filter.Eq(a => a.Id, objectId)).SingleAsync();
        }

        public async Task Remove(DateTime before)
        {
            await _auditCollection.DeleteManyAsync(Builders<Audit>.Filter.Lt(x => x.TimeSent, before));
        }

        public Task<List<Audit>> Find(Guid correlationId)
        {
            return _auditCollection.Find(Builders<Audit>.Filter.Eq(x => x.CorrelationId, correlationId)).Sort(Builders<Audit>.Sort.Descending(x => x.TimeSent)).ToListAsync();
        }

        public async Task InsertAudit(Audit model)
        {
            await _auditCollection.InsertOneAsync(model);

            await _serviceMessagesCollection.UpdateOneAsync(
                Builders<ServiceMessage>.Filter.And(
                    Builders<ServiceMessage>.Filter.Eq(x => x.In, model.DestinationAddress),
                    Builders<ServiceMessage>.Filter.Eq(x => x.Out, model.SourceAddress)
                ),
                Builders<ServiceMessage>.Update
                    .Inc(x => x.Count, 1)
                    .Set(x => x.LastSent, model.TimeReceived)
                    .Set(x => x.Type, model.TypeName),
                new UpdateOptions {IsUpsert = true});
        }

        public Task<List<Audit>> Find(DateTime @from, DateTime to)
        {
            //var result = _auditCollection
            //    .AsQueryable()
            //    .Where(x => x.TimeSent >= from && x.TimeSent <= to)
            //   .OrderByDescending(x => x.TimeSent);

            return _auditCollection
                .Find(Builders<Audit>.Filter.And(
                    Builders<Audit>.Filter.Gte(x => x.TimeSent, from),
                    Builders<Audit>.Filter.Lte(x => x.TimeSent, to)))
                .Sort(Builders<Audit>.Sort.Descending(x => x.TimeSent))
                .ToListAsync();
        }
    }
}