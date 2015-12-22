using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQRS.Infrastructure.Sql.Messaging
{
    public class MessageReceivedEventArgs:EventArgs
    {
        public MessageReceivedEventArgs(Message message)
        {
            this.Message = message;
        }

        public Message Message { get; private set; }
    }
}
