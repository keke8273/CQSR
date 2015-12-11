using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.Messaging.Handling
{
    public class EventDispatcher
    {
        private Dictionary<Type, List<Tuple<Type, Action<Envelope>>>> handlersByEventType;
        private Dictionary<Type, Action<IEvent, string, string, string>> dispatchersByEventType;

        public EventDispatcher()
        {
            this.handlersByEventType = new Dictionary<Type, List<Tuple<Type, Action<Envelope>>>>();
            this.dispatchersByEventType = new Dictionary<Type, Action<IEvent, string, string, string>>();
        }

        public EventDispatcher(IEnumerable<IEventHandler> handlers)
            : this()
        {
            foreach (var handler in handlers)
            {
                this.Register(handler);
            }
        }

        public void Register(IEventHandler handler)
        {
            var handlerType = handler.GetType();

            foreach (var invocationTuple in this.BuildHandlerInvocations(handler))
            {
                var envelopType = typeof(Envelope<>).MakeGenericType(invocationTuple.Item1);
            }
        }

        private IEnumerable<Tuple<Type, Action<Envelope>>> BuildHandlerInvocations(IEventHandler handler)
        {
            var interfaces = handler.GetType().GetInterfaces();

            var eventHandlerInvocations =
                interfaces
                     .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                     .Select(i => new { HandlerInterface = i, EventType = i.GetGenericArguments()[0] })
                     .Select(e => new Tuple<Type, Action<Envelope>>(e.EventType, this.BuildHandlerInvocation(handler, e.HandlerInterface, e.EventType)));
                
        }

        private Action<Envelope> BuildHandlerInvocation(IEventHandler handler, Type handlerType, Type messageType)
        {

        }
    }
}
