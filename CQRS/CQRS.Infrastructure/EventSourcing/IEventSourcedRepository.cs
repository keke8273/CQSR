using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.EventSourcing
{
    public interface IEventSourcedRepository<T> where T: IEventSourced
    {
        T Find(Guid id);

        T Get(Guid id);

        /// <summary>
        /// Saves the specified event sourced.
        /// </summary>
        /// <param name="eventSourced">The event sourced entity.</param>
        /// <param name="correlationId">The correlation id that identifies the command that generates the event.</param>
        void Save(T eventSourced, string correlationId);
    }
}
