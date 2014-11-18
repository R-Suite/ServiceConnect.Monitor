using System;
using R.MessageBus.Interfaces;

namespace R.MessageBus.AcceptanceTests
{
    public class TestErrorMessage : Message
    {
        public TestErrorMessage(Guid correlationId)
            : base(correlationId)
        {
        }

        public string Name { get; set; }
    }
}