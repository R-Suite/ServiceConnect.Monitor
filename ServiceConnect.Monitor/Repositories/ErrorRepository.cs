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
    public class ErrorRepository : IErrorRepository
    {
        private readonly IMongoCollection<Error> _errorCollection;

        public ErrorRepository(IMongoRepository mongoRepository, string errorCollectionName)
        {
            _errorCollection = mongoRepository.Database.GetCollection<Error>(errorCollectionName);
        }

        public async Task InsertError(Error model)
        {
            await _errorCollection.InsertOneAsync(model);
        }
        public async Task EnsureIndex()
        {
            await _errorCollection.Indexes.CreateOneAsync(Builders<Error>.IndexKeys.Ascending(x => x.TimeSent));
        }

        public Task<List<Error>> Find(DateTime @from, DateTime to)
        {
            return _errorCollection.Find(
                Builders<Error>.Filter.And(
                    Builders<Error>.Filter.Gte(x => x.TimeSent, from),
                    Builders<Error>.Filter.Lte(x => x.TimeSent, to))
                ).ToListAsync();
        }

        public Task<Error> Get(Guid id)
        {
            return _errorCollection.Find(Builders<Error>.Filter.Eq(x => x.Id, id)).SingleAsync();
        }

        public Task Remove(DateTime before)
        {
            return _errorCollection.DeleteManyAsync(Builders<Error>.Filter.Lt(x => x.TimeSent, before));
        }

        public async Task Remove(Guid id)
        {
            await _errorCollection.DeleteOneAsync(Builders<Error>.Filter.Eq(x => x.Id, id));
        }

        public Task<List<Error>> Find(Guid correlationId)
        {
            return _errorCollection.Find(Builders<Error>.Filter.Eq(x => x.CorrelationId, correlationId)).ToListAsync();
        }
    }
}