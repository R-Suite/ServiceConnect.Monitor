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

using MongoDB.Driver;
using ServiceConnect.Monitor.Interfaces;
using ServiceConnect.Monitor.Models;

namespace ServiceConnect.Monitor.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly MongoCollection<Settings> _settingsRepository;

        public SettingsRepository(IMongoRepository mongoRepository, string settingsCollectionName)
        {
            _settingsRepository = mongoRepository.Database.GetCollection<Settings>(settingsCollectionName);
        }
        
        public Settings Get()
        {
            return _settingsRepository.FindOne();
        }

        public void Update(Settings model)
        {
            _settingsRepository.Save(model);
        }
    }
}