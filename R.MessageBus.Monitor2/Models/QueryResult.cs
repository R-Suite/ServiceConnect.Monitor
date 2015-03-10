using System.Collections.Generic;

namespace R.MessageBus.Monitor.Models
{
    public class QueryResult<T>
    {
        public int Count { get; set; }
        public List<T> Results { get; set; } 
    }
}