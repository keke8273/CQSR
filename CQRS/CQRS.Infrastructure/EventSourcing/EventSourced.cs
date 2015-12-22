namespace CQRS.Infrastructure.EventSourcing
{
    using System;
    using System.Collections.Generic;
    using Messaging;

    public class EventSourced : IEventSourced      
    {
        #region Member Variables
		
        private readonly Dictionary<Type, Action<IVersionedEvent>> handler = new Dictionary<Type, Action<IVersionedEvent>>();
        private readonly List<IVersionedEvent> pendingEvents = new List<IVersionedEvent>();

        private readonly Guid id;
        private int version = -1;
        #endregion

        public EventSourced(Guid id)
        {
            this.id = id;
        }

        #region Properties
        public Guid Id
        {
            get { return id; }
        }

        /// <summary>
        /// Gets the entity's version. As the entity is being updated and events being generated, the version is incremented.
        /// </summary>
        public int Version
        {
            get { return version; }
            protected set { version = value; }
        }

        /// <summary>
        /// Gets the collection of new events since the entity was loaded, as a consequence of command handling.
        /// </summary>
        public IEnumerable<IVersionedEvent> Events
        {
            get { return pendingEvents; }
        } 
        #endregion

        #region Methods
        /// <summary>
        /// Registers a specified handler for the type of event.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">The handler.</param>
        protected void RegisterHandler<TEvent>(Action<TEvent> handler) 
            where TEvent : IEvent
        {
            this.handler.Add(typeof(TEvent), @event => handler((TEvent)@event));
        }

        protected void LoadFrom(IEnumerable<IVersionedEvent> pastEvents)
        {
            foreach (var e in pastEvents)
            {
                handler[e.GetType()].Invoke(e);
                version = e.Version;
            }
        }

        /// <summary>
        /// Updates the event sourced entity with a new event. Entity state may be updated 
        /// </summary>
        protected void Update(VersionedEvent e)
        {
            e.SourceId = id;
            e.Version = version + 1;
            handler[e.GetType()].Invoke(e);
            version = e.Version;
            pendingEvents.Add(e);
        }

        #endregion
    }
}
