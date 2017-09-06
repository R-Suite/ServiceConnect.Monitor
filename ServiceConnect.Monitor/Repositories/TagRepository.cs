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
using Tag = ServiceConnect.Monitor.Models.Tag;

namespace ServiceConnect.Monitor.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly IMongoCollection<Models.Tag> _tagCollection;

        public TagRepository(IMongoRepository mongoRepository, string tagCollectionName)
        {
            _tagCollection = mongoRepository.Database.GetCollection<Models.Tag>(tagCollectionName);
        }

        public Task<List<Tag>> Find()
        {
            return _tagCollection.Find(FilterDefinition<Tag>.Empty).ToListAsync();
        }

        public async Task Insert(string tag)
        {
            var model = new Tag
            {
                Name = tag,
                Id = Guid.NewGuid()
            };
            await _tagCollection.InsertOneAsync(model);
        }
    }
}