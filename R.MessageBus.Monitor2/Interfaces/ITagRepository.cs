using System.Collections.Generic;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Interfaces
{
    public interface ITagRepository
    {
        List<Tag> Find();
        void Insert(string tag);
    }
}