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
using ServiceConnect.Monitor.Interfaces;
using ServiceConnect.Monitor.Models;

namespace ServiceConnect.Monitor.Repositories
{
    public class ErrorRepository : IErrorRepository
    {
        private readonly MongoCollection<Error> _errorCollection;

        public ErrorRepository(string mongoConnectionString)
        {
            var mongoClient = new MongoClient(mongoConnectionString);
            MongoServer server = mongoClient.GetServer();
            var mongoDatabase = server.GetDatabase("RMessageBusMonitor");
            _errorCollection = mongoDatabase.GetCollection<Error>("Error");
        }

        public void InsertError(Error model)
        {
            _errorCollection.Insert(model);
        }
        public void EnsureIndex()
        {
            _errorCollection.CreateIndex(IndexKeys<Error>.Ascending(x => x.TimeSent));
        }

        public IList<Error> Find(DateTime @from, DateTime to)
        {
            var result = _errorCollection.AsQueryable().Where(x =>
               x.TimeSent >= from &&
               x.TimeSent <= to).OrderByDescending(x => x.TimeSent);

            return result.ToList();
        }

        public Error Get(Guid id)
        {        	
            return _errorCollection.FindOneById(id);
        }

        public void Remove(DateTime before)
        {
            _errorCollection.Remove(Query<Error>.LT(x => x.TimeSent, before));
        }

        public void Remove(Guid id)
        {
            _errorCollection.Remove(Query<Error>.EQ(x => x.Id, id));
        }

        public IList<Error> Find(Guid correlationId)
        {
            return _errorCollection.Find(Query<Error>.EQ(x => x.CorrelationId, correlationId)).ToList();
        }
    }
}