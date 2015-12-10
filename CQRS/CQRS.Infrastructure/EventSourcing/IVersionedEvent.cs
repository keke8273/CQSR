using CQRS.Infrastructure.Messaging;

namespace CQRS.Infrastructure.EventSourcing
{
    public interface IVersionedEvent : IEvent
    {
        /// <summary>
        /// Gets the version of the event in the stream.
        /// </summary>
        int Version { get; }
    }
}