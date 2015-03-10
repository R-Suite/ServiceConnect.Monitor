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
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly MongoCollection<Settings> _settingsRepository;

        public SettingsRepository(string mongoConnectionString)
        {
            var mongoClient = new MongoClient(mongoConnectionString);
            MongoServer server = mongoClient.GetServer();
            var mongoDatabase = server.GetDatabase("RMessageBusMonitor");
            _settingsRepository = mongoDatabase.GetCollection<Settings>("Settings");
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