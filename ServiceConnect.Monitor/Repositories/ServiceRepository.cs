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
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using ServiceConnect.Monitor.Interfaces;
using ServiceConnect.Monitor.Models;

namespace ServiceConnect.Monitor.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly MongoCollection<Service> _serviceCollection;

        public ServiceRepository(string mongoConnectionString)
        {
            var mongoClient = new MongoClient(mongoConnectionString);
            MongoServer server = mongoClient.GetServer();
            var mongoDatabase = server.GetDatabase("RMessageBusMonitor");
            _serviceCollection = mongoDatabase.GetCollection<Service>("Services");
        }

        public void EnsureIndex()
        {
            _serviceCollection.CreateIndex(IndexKeys<Service>.Ascending(x => x.Name).Ascending(x => x.InstanceLocation));
            _serviceCollection.CreateIndex(IndexKeys<Service>.Ascending(x => x.Name));
            _serviceCollection.CreateIndex(IndexKeys<Service>.Ascending(x => x.Tags));
        }

        public IList<Service> Find()
        {
            return _serviceCollection.AsQueryable().ToList();
        }

        public Service Find(string name, string location)
        {
            return _serviceCollection.AsQueryable().FirstOrDefault(x => x.Name == name && x.InstanceLocation == location);
        }

        public IList<Service> FindByName(string name)
        {
            return _serviceCollection.AsQueryable().Where(x => x.Name == name).ToList();
        }

        public void Update(Service model)
        {
            _serviceCollection.Save(model);
        }

        public Service Get(ObjectId id)
        {
            return _serviceCollection.FindOneById(id);
        }

        public void Delete(ObjectId id)
        {
            _serviceCollection.Remove(Query<Service>.EQ(x => x.Id, id));
        }
    }
}