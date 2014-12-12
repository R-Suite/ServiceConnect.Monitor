using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Interfaces
{
    public interface ISettingsRepository
    {
        Settings Get();
        void Update(Settings settings);
    }
}