using System.Collections.Generic;

namespace CQRS.Infrastructure.Messaging
{
    public interface IEventPublisher
    {
        IEnumerable<IEvent> Events { get; } 
    }
}
