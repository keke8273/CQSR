using System.IO;
using CQRS.Infrastructure.Messaging;
using CQRS.Infrastructure.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.Sql.Messaging
{
    public class EventBus : IEventBus
    {
        private readonly IMessageSender _sender;
        private readonly ITextSerializer _serializer;

        public EventBus(IMessageSender sender, ITextSerializer serializer)
        {
            _sender = sender;
            _serializer = serializer;
        }

        public void Publish(Envelope<IEvent> @event)
        {
            var message = BuildMessage(@event);

            _sender.Send(message);
        }

        public void Publish(IEnumerable<Envelope<IEvent>> events)
        {
            var messages = events.Select(e => BuildMessage(e));
            
            _sender.Send(messages);
        }

        private Message BuildMessage(Envelope<IEvent> @event)
        {
            using (var payloadWriter = new StringWriter())
            {
                _serializer.Serialize(payloadWriter, @event.Body);
                //to delivery date for events
                return new Message(payloadWriter.ToString(), correlationId: @event.CorrelationId);
            }
        }
    }
}
