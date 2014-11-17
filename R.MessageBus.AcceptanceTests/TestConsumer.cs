using R.MessageBus.Interfaces;

namespace R.MessageBus.AcceptanceTests
{
    public class TestConsumer : IMessageHandler<TestMessage>
    {
        public void Execute(TestMessage message)
        {
        }

        public IConsumeContext Context { get; set; }
    }
}