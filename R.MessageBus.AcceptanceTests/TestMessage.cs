using System;
using R.MessageBus.Interfaces;

namespace R.MessageBus.AcceptanceTests
{
    public class TestMessage : Message
    {
        public TestMessage(Guid correlationId) : base(correlationId)
        {
        }

        public string Name { get; set; }
    }
}