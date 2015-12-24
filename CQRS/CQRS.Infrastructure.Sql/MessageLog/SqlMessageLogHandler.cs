using CQRS.Infrastructure.Messaging;
using CQRS.Infrastructure.Messaging.Handling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.Sql.MessageLog
{
    /// <summary>
    /// Implement as a generic IEvent, ICommand handler that will capture all events and commands 
    /// generated in the system.
    /// </summary>
    public class SqlMessageLogHandler : IEventHandler<IEvent>, ICommandHandler<ICommand>
    {
        private SqlMessageLog log;
        public SqlMessageLogHandler(SqlMessageLog log)
        {
            this.log = log;
        }

        public void Handle(IEvent @event)
        {
            this.log.Save(@event);
        }

        public void Handle(ICommand command)
        {
            this.log.Save(command);
        }
    }
}
