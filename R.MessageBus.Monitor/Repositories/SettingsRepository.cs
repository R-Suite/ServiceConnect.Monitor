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