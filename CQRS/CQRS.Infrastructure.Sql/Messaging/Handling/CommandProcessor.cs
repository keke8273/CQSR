using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CQRS.Infrastructure.Messaging;
using CQRS.Infrastructure.Messaging.Handling;
using CQRS.Infrastructure.Serialization;

namespace CQRS.Infrastructure.Sql.Messaging.Handling
{
    public class CommandProcessor : MessageProcessor, ICommandHandlerRegistry
    {
        private readonly Dictionary<Type, ICommandHandler> _handlers = new Dictionary<Type, ICommandHandler>();
 
        public CommandProcessor(IMessageReceiver receiver, ITextSerializer serializer)
            : base(receiver, serializer)
        {
        }

        protected override void ProcessMessage(object payload, string correlationId)
        {
            var commandType = payload.GetType();
            ICommandHandler handler = null;

            if (_handlers.TryGetValue(commandType, out handler))
            {
                Trace.WriteLine("-- Handled by " + handler.GetType().FullName);
                ((dynamic) handler).Handle((dynamic) payload);
            }

            if (_handlers.TryGetValue(typeof(ICommand), out handler))
            {
                Trace.WriteLine("-- Handled by " + handler.GetType().FullName);
                ((dynamic) handler).Handle((dynamic) payload);
            }
        }

        public void Register(ICommandHandler commandHandler)
        {
            var genericHandler = typeof (ICommandHandler<>);
            var supportedCommandTypes = commandHandler.GetType()
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericHandler)
                .Select(i => i.GetGenericArguments()[0])
                .ToList();

            if(_handlers.Keys.Any(registeredType => supportedCommandTypes.Contains(registeredType)))
                throw new ArgumentException("The command handled by the received handler already has a registered handler.");

            foreach (var commandType in supportedCommandTypes)
            {
                _handlers.Add(commandType, commandHandler);
            }
        }
    }
}
