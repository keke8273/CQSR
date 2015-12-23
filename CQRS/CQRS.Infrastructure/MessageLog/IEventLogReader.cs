using CQRS.Infrastructure.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.MessageLog
{
    /// <summary>
    /// Exposes the message log for all events that the system processed.
    /// </summary>
    public interface IEventLogReader
    {
        IEnumerable<IEvent> Query(QueryCriteria criteria);
    }
}
