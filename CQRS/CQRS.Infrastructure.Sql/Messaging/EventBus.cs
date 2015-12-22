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
        private readonly IMessageSender sender;
        private readonly ITextSerializer serializer;

        public EventBus(IMessageSender sender, ITextSerializer serialzier)
        {
            this.sender = sender;
            this.serializer = serializer;
        }

        public void Publish(Envelope<IEvent> @event)
        {
            var message = this.BuildMessage(@event);

            this.sender.Send(message);
        }

        public void Publish(IEnumerable<Envelope<IEvent>> events)
        {
            throw new NotImplementedException();
        }

        private Message BuildMessage(Envelope<IEvent> @event)
        {
            using (var payloadWriter = new System.IO.StringWriter())
            {
                this.serializer.Serialize(payloadWriter, @event.Body);
                //to delivery date for events
                return new Message(payloadWriter.ToString(), correlationId: @event.CorrelationId);
            }
        }
    }
}
