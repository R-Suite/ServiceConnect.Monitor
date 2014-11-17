using System.Collections.Generic;

namespace R.MessageBus.Monitor.Models
{
    public class QueryResult<T>
    {
        public long Count { get; set; }
        public List<T> Results { get; set; } 
    }
}