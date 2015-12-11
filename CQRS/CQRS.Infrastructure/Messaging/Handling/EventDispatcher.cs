using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.Messaging.Handling
{
    public class EventDispatcher
    {
        //first Type is the event type which is the key, while the second Type is the handler type.
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

                List<Tuple<Type, Action<Envelope>>> invocations;

                if (!this.handlersByEventType.TryGetValue(invocationTuple.Item1, out invocations))
                {
                    invocations = new List<Tuple<Type, Action<Envelope>>>();
                    this.handlersByEventType[invocationTuple.Item1] = invocations;
                }
                invocations.Add(new Tuple<Type, Action<Envelope>>(handlerType, invocationTuple.Item2));

                if (!this.dispatchersByEventType.ContainsKey(invocationTuple.Item1))
                {
                    this.dispatchersByEventType[invocationTuple.Item1] = this.BuildDispatchInvocation(invocationTuple.Item1);
                }
            }
        }

        //distill the events that the handler handles and build a invocation map
        private IEnumerable<Tuple<Type, Action<Envelope>>> BuildHandlerInvocations(IEventHandler handler)
        {
            var interfaces = handler.GetType().GetInterfaces();

            var eventHandlerInvocations =
                interfaces
                     .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                     .Select(i => new { HandlerInterface = i, EventType = i.GetGenericArguments()[0] })
                     .Select(e => new Tuple<Type, Action<Envelope>>(e.EventType, this.BuildHandlerInvocation(handler, e.HandlerInterface, e.EventType)));

            return eventHandlerInvocations;
        }

        private Action<Envelope> BuildHandlerInvocation(IEventHandler handler, Type handlerType, Type messageType)
        {
            var envelopeType = typeof(Envelope<>).MakeGenericType(messageType);

            var parameter = Expression.Parameter(typeof(Envelope));
            var invocationExpression =
                Expression.Lambda(
                    Expression.Block(
                        Expression.Call(
                            Expression.Convert(Expression.Constant(hander), handlerType),
                            handlerType.GetMethod("Handle"),
                            Expression.Property(Expression.Convert(parameter, envelopeType), "Body"))),
                    parameter);

            return (Action<Envelope>)invocationExpression.Compile();
        }
    }
}
