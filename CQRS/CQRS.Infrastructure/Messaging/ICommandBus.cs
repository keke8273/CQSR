using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.Messaging
{
    public interface ICommandBus
    {
        using System.Collections.Generic;

        void Send(Envelope<ICommand> command);
        void Send(IEnumerable<Envelope<ICommand>> commands);
    }
}
