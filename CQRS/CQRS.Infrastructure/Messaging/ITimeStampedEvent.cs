using System;

namespace CQRS.Infrastructure.Messaging
{
    public interface ITimeStampedEvent : IEvent
    {
        DateTime TimeStamp { get; set; }
    }
}
