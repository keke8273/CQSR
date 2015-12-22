using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQRS.Infrastructure.Sql.Messaging
{
    public interface IMessageSender
    {
        void Send(Message message);

        void Send(IEnumerable<Message> messages);
    }
}
