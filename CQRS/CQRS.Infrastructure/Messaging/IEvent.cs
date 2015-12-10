
namespace CQRS.Infrastructure.Messaging
{
    using System;
    /// <summary>
    /// Represents an event message.
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Gets the source identifier of the source originating the event.
        /// </summary>
        Guid SourceId { get; }
    }
}