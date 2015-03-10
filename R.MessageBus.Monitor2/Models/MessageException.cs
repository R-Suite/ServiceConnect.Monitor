using System;

namespace R.MessageBus.Monitor.Models
{
    public class MessageException
    {
        public DateTime Timestamp { get; set; }
        public string ExceptionType { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string Source { get; set; }
        public string Exception { get; set; }
    }
}