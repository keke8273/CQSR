using CQRS.Infrastructure.Messaging;
using CQRS.Infrastructure.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.Sql.Messaging
{
    public class CommandBus : ICommandBus
    {
        private readonly IMessageSender sender;
        private readonly ITextSerializer serializer;

        public CommandBus(IMessageSender sender, ITextSerializer serializer)
        {
            this.sender = sender;
            this.serializer = serializer;
        }

        public void Send(Envelope<ICommand> command)
        {
            var message = BuildMessage(command);
            sender.Send(message);
        }

        public void Send(IEnumerable<Envelope<ICommand>> commands)
        {
            var messages = commands.Select(command => BuildMessage(command));
            sender.Send(messages);
        }

        private Message BuildMessage(Envelope<ICommand> command)
        {
            using (var payloadWriter = new StringWriter())
            {
                serializer.Serialize(payloadWriter, command.Body);
                return new Message(payloadWriter.ToString(), command.Delay != TimeSpan.Zero ? (DateTime?)DateTime.UtcNow.Add(command.Delay) : null, command.CorrelationId);
            }
        }
    }
}
