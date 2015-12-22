using CQRS.Infrastructure.Messaging;
using CQRS.Infrastructure.Messaging.Handling;
using CQRS.Infrastructure.Serialization;

namespace CQRS.Infrastructure.Sql.Messaging.Handling
{
    public class EventProcessor : MessageProcessor, IEventHandlerRegistery
    {
        private readonly EventDispatcher _messageDispatcher = new EventDispatcher();

        public EventProcessor(IMessageReceiver receiver, ITextSerializer serializer) : base(receiver, serializer)
        {
        }

        protected override void ProcessMessage(object payload, string correlationId)
        {
            var @event = (IEvent) payload;
            _messageDispatcher.DispatchMessage(@event, null, correlationId, "");
        }

        public void Register(IEventHandler eventHandler)
        {
            _messageDispatcher.Register(eventHandler);
        }
    }
}
