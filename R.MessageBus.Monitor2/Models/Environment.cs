namespace R.MessageBus.Monitor.Models
{
    public class Environment
    {
        public string Server { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ErrorQueue { get; set; }
        public string AuditQueue { get; set; }
        public string HeartbeatQueue { get; set; }
    }
}