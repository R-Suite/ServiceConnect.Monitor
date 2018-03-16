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
using MongoDB.Driver;
using ServiceConnect.Monitor.Interfaces;
using ServiceConnect.Monitor.Models;

namespace ServiceConnect.Monitor.Repositories
{
    public class HeartbeatRepository : IHeartbeatRepository
    {
        private readonly IMongoCollection<Heartbeat> _heartbeatsCollection;
        private readonly IMongoCollection<Service> _serviceCollection;

        public HeartbeatRepository(IMongoRepository mongoRepository, string heartbeatCollectionName, string serviceCollectionName)
        {
            _heartbeatsCollection = mongoRepository.Database.GetCollection<Heartbeat>(heartbeatCollectionName);
            _serviceCollection = mongoRepository.Database.GetCollection<Service>(serviceCollectionName);
        }

        public async Task InsertHeartbeat(Heartbeat model)
        {
            await _heartbeatsCollection.InsertOneAsync(model);

            await _serviceCollection.UpdateOneAsync(
                Builders<Service>.Filter.And(
                    Builders<Service>.Filter.Eq(x => x.Name, model.Name),
                    Builders<Service>.Filter.Eq(x => x.InstanceLocation, model.Location)
                ),
                Builders<Service>.Update
                    .Set(x => x.Language, model.Language)
                    .Set(x => x.ConsumerType, model.ConsumerType)
                    .Set(x => x.LastHeartbeat, model.Timestamp)
                    .Set(x => x.LatestCpu, model.LatestCpu)
                    .Set(x => x.LatestMemory, model.LatestMemory),
                new UpdateOptions {IsUpsert = true});
        }

        public Task<List<Heartbeat>> Find(string name, string location, DateTime @from, DateTime to)
        {
            return _heartbeatsCollection
                .Find(
                    Builders<Heartbeat>.Filter.And(
                        Builders<Heartbeat>.Filter.Gte(x => x.Timestamp, from),
                        Builders<Heartbeat>.Filter.Lte(x => x.Timestamp, to),
                        Builders<Heartbeat>.Filter.Eq(x => x.Name, name),
                        Builders<Heartbeat>.Filter.Eq(x => x.Location, location)))
                .Sort(Builders<Heartbeat>.Sort.Descending(x => x.Timestamp))
                .ToListAsync();
        }

        public async Task Remove(string name, string location)
        {
            await _heartbeatsCollection.DeleteManyAsync(
                Builders<Heartbeat>.Filter.And(
                    Builders<Heartbeat>.Filter.Eq(x => x.Name, name),
                    Builders<Heartbeat>.Filter.Eq(x => x.Location, location)
                )
            );
        }

        public async Task EnsureIndex()
        {
            await _heartbeatsCollection.Indexes.CreateOneAsync(Builders<Heartbeat>.IndexKeys.Ascending(x => x.Name).Ascending(x => x.Location).Descending(x => x.Timestamp));
            await _heartbeatsCollection.Indexes.CreateOneAsync(Builders<Heartbeat>.IndexKeys.Ascending(x => x.Name).Ascending(x => x.Location));
        }

        public async Task Remove(DateTime before)
        {
            await _heartbeatsCollection.DeleteManyAsync(Builders<Heartbeat>.Filter.Lt(x => x.Timestamp, before));
        }
    }
}