﻿using CQRS.Infrastructure.Messaging;

namespace CQRS.Infrastructure.EventSourcing
{
    public interface IVersionedEvent : IEvent
    {
        /// <summary>
        /// Gets the version or order of the event in the stream.
        /// </summary>
        int Version { get; }
    }
}