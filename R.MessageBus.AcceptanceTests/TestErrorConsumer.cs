using System;
using R.MessageBus.Interfaces;

namespace R.MessageBus.AcceptanceTests
{
    public class TestErrorConsumer : IMessageHandler<TestErrorMessage>
    {
        public void Execute(TestErrorMessage message)
        {
            throw new Exception("Error handling test message");
        }

        public IConsumeContext Context { get; set; }
    }
}