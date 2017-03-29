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
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using ServiceConnect.Monitor.Interfaces;
using ServiceConnect.Monitor.Models;

namespace ServiceConnect.Monitor.Repositories
{
    public class HeartbeatRepository : IHeartbeatRepository
    {
        private readonly MongoCollection<Heartbeat> _heartbeatsCollection;
        private readonly MongoCollection<Service> _serviceCollection;

        public HeartbeatRepository(IMongoRepository mongoRepository, string heartbeatCollectionName, string serviceCollectionName)
        {
            _heartbeatsCollection = mongoRepository.Database.GetCollection<Heartbeat>(heartbeatCollectionName);
            _serviceCollection = mongoRepository.Database.GetCollection<Service>(serviceCollectionName);
        }

        public void InsertHeartbeat(Heartbeat model)
        {
            _heartbeatsCollection.Insert(model);

            _serviceCollection.Update(
                Query.And(
                    Query<Service>.EQ(x => x.Name, model.Name),
                    Query<Service>.EQ(x => x.InstanceLocation, model.Location)
                ),
                Update<Service>.Set(x => x.Language, model.Language).
                                Set(x => x.ConsumerType, model.ConsumerType).
                                Set(x => x.LastHeartbeat, model.Timestamp).
                                Set(x => x.LatestCpu, model.LatestCpu).
                                Set(x => x.LatestMemory, model.LatestMemory),
                UpdateFlags.Upsert);
        }

        public List<Heartbeat> Find(string name, string location, DateTime @from, DateTime to)
        {
            var result = _heartbeatsCollection.AsQueryable().Where(x => 
                x.Timestamp >= from &&
                x.Timestamp <= to && 
                x.Name == name &&
                x.Location == location).OrderByDescending(x => x.Timestamp);

            return result.ToList();
        }

        public void Remove(string name, string location)
        {
            _heartbeatsCollection.Remove(
                Query.And(
                    Query<Heartbeat>.EQ(x => x.Name, name),
                    Query<Heartbeat>.EQ(x => x.Location, location)
                )
            );
        }

        public void EnsureIndex()
        {
            _heartbeatsCollection.CreateIndex(IndexKeys<Heartbeat>.Ascending(x => x.Name).Ascending(x => x.Location).Descending(x => x.Timestamp));
            _heartbeatsCollection.CreateIndex(IndexKeys<Heartbeat>.Ascending(x => x.Name).Ascending(x => x.Location));
        }

        public void Remove(DateTime before)
        {
            _heartbeatsCollection.Remove(Query<Heartbeat>.LT(x => x.Timestamp, before));
        }
    }
}