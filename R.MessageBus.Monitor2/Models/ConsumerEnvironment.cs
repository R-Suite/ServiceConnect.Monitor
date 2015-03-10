using R.MessageBus.Monitor.Handlers;

namespace R.MessageBus.Monitor.Models
{
    public class ConsumerEnvironment
    {
        public string Server { get; set; }
        public AuditMessageHandler AuditMessageHandler { get; set; }
        public ErrorMessageHandler ErrorMessageHandler { get; set; }
        public HearbeatMessageHandler HeartbeatMessageHandler { get; set; }
        public Consumer AuditConsumer { get; set; }
        public Consumer ErrorConsumer { get; set; }
        public Consumer HeartbeatConsumer { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string AuditQueue { get; set; }
        public string ErrorQueue { get; set; }
        public string HeartbeatQueue { get; set; }
        public Producer Producer { get; set; }
    }
}