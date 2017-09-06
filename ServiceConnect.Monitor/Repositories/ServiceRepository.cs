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

using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using ServiceConnect.Monitor.Interfaces;
using ServiceConnect.Monitor.Models;

namespace ServiceConnect.Monitor.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly IMongoCollection<Service> _serviceCollection;

        public ServiceRepository(IMongoRepository mongoRepository, string servicesCollectionName)
        {
            _serviceCollection = mongoRepository.Database.GetCollection<Service>(servicesCollectionName);
        }

        public async Task EnsureIndex()
        {
            await _serviceCollection.Indexes.CreateOneAsync(Builders<Service>.IndexKeys.Ascending(x => x.Name).Ascending(x => x.InstanceLocation));
            await _serviceCollection.Indexes.CreateOneAsync(Builders<Service>.IndexKeys.Ascending(x => x.Name));
            await _serviceCollection.Indexes.CreateOneAsync(Builders<Service>.IndexKeys.Ascending(x => x.Tags));
        }

        public Task<List<Service>> Find()
        {
            return _serviceCollection.Find(FilterDefinition<Service>.Empty).ToListAsync();
        }

        public Task<Service> Find(string name, string location)
        {
            return _serviceCollection.Find(
                    Builders<Service>.Filter.And(
                        Builders<Service>.Filter.Eq(x => x.Name, name),
                        Builders<Service>.Filter.Eq(x => x.InstanceLocation, location)))
                .SingleAsync();
        }

        public Task<List<Service>> FindByName(string name)
        {
            return _serviceCollection.Find(Builders<Service>.Filter.Eq(x => x.Name, name)).ToListAsync();
        }

        public async Task Update(Service model)
        {
            await _serviceCollection.ReplaceOneAsync(Builders<Service>.Filter.Eq(x => x.Id, model.Id), model, new UpdateOptions {IsUpsert = true});
        }

        public Task<Service> Get(ObjectId id)
        {
            return _serviceCollection.Find(Builders<Service>.Filter.Eq(x => x.Id, id)).SingleAsync();
        }

        public async Task Delete(ObjectId id)
        {
            await _serviceCollection.DeleteOneAsync(Builders<Service>.Filter.Eq(x => x.Id, id));
        }
    }
}