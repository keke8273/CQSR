
namespace CQRS.Infrastructure.EventSourcing
{
    using System;

    public abstract class VersionedEvent : IVersionedEvent
    {
        /// <summary>
        /// Gets the source identifier of the source originating the event.
        /// </summary>
        public Guid SourceId { get; set; }

        public int Version { get; set; }
    }
}
