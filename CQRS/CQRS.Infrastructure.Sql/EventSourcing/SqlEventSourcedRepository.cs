using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CQRS.Infrastructure.EventSourcing;
using CQRS.Infrastructure.Messaging;
using CQRS.Infrastructure.Serialization;

namespace CQRS.Infrastructure.Sql.EventSourcing
{
    public class SqlEventSourcedRepository<T> : IEventSourcedRepository<T> where T : class, IEventSourced
    {
        private static readonly string sourceType = typeof (T).Name;
        private readonly IEventBus eventBus;
        private readonly ITextSerializer serializer;
        private readonly Func<EventStoreDbContext> contextFactory;
        private readonly Func<Guid, IEnumerable<IVersionedEvent>, T> entityFactory;

        public SqlEventSourcedRepository(IEventBus eventBus, ITextSerializer serializer, Func<EventStoreDbContext> contextFactory)
        {
            this.eventBus = eventBus;
            this.serializer = serializer;
            this.contextFactory = contextFactory;

            var constructor = typeof (T).GetConstructor(new[] {typeof (Guid), typeof (IEnumerable<IVersionedEvent>)});

            if (constructor == null)
            {
                throw new InvalidCastException("Type T must have a constructor with the following signature: .ctor(Guid, IEnumberable<IVersionedEvent>)");
            }

            entityFactory = (id, events) => (T) constructor.Invoke(new object[] {id, events});
        }

        public T Find(Guid id)
        {
            using (var context = contextFactory.Invoke())
            {
                var deserialized = context.Set<Event>()
                    .Where(x => x.AggregateId == id && x.AggregateType == sourceType)
                    .OrderBy(x => x.Version)
                    .AsEnumerable()
                    .Select(Deserialize);

                if (deserialized.Any())
                {
                    return entityFactory.Invoke(id, deserialized);
                }

                return null;
            }
        }

        public T Get(Guid id)
        {
            var entity = Find(id);
            if (entity == null)
            {
                throw new EntityNotFoundException(id, sourceType);
            }

            return entity;
        }

        public void Save(T eventSourced, string correlationId)
        {
            var events = eventSourced.Events.ToArray();

            using (var context = contextFactory.Invoke())
            {
                var serialized = eventSourced.Events.Select(e => Serializer(e, correlationId));

                context.Set<Event>().AddRange(serialized);

                context.SaveChanges();
            }

            eventBus.Publish(events.Select(e => new Envelope<IEvent>(e){CorrelationId = correlationId}));
        }

        private Event Serializer(IVersionedEvent e, string correlationId)
        {
            Event serialized;

            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, e);
                serialized = new Event
                {
                    AggregateId = e.SourceId,
                    AggregateType = sourceType,
                    Version = e.Version,
                    Payload = writer.ToString(),
                    CorrelationId = correlationId
                };
            }

            return serialized;
        }

        private IVersionedEvent Deserialize(Event @event)
        {
            using (var reader = new StringReader(@event.Payload))
            {
                return (IVersionedEvent)serializer.Deserialize(reader);
            }
        }
    }
}
