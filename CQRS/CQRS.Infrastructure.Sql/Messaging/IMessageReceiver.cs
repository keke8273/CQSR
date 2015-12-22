using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.Sql.Messaging
{
    public interface IMessageReceiver
    {
        event EventHandler<MessageReceivedEventArgs> MessageReceived;

        void Start();

        void Stop();
    }
}
