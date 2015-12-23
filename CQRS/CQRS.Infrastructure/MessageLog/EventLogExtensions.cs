using CQRS.Infrastructure.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.MessageLog
{
    public static class EventLogExtensions
    {
        public static IEnumerable<IEvent> ReadAll(this IEventLogReader log)
        {
            return log.Query(new QueryCriteria());
        }

        public static IEventQuery Query(this IEventLogReader log)
        {
            return new EventQuery(log);
        }

        public partial interface IEventQuery : IEnumerable<IEvent>
        {
            /// <summary>
            /// Executes the query built using the fluent API against the underlying store.
            /// </summary>
            IEnumerable<IEvent> Execute();

            IEventQuery WithTypeName(string typeName);

            IEventQuery WithFullName(string fullName);

            IEventQuery FromAssembly(string assemblyName);

            IEventQuery FromNamespace(string @namespace);

            IEventQuery FromSource(string sourceType);

            IEventQuery Until(DateTime endDate);
        }

        private class EventQuery : IEventQuery, IEnumerable<IEvent>
        {
            private IEventLogReader log;
            private QueryCriteria criteria = new QueryCriteria();

            public EventQuery(IEventLogReader log)
            {
                this.log = log;
            }

            public IEnumerable<IEvent> Execute()
            {
                return log.Query(criteria);
            }

            //IEnumberable implementation
            public IEnumerator<IEvent> GetEnumerator()
            {
                return this.Execute().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            public IEventQuery WithTypeName(string typeName)
            {
                criteria.TypeNames.Add(typeName);
                return this;
            }

            public IEventQuery WithFullName(string fullName)
            {
                criteria.FullNames.Add(fullName);
                return this;
            }

            public IEventQuery FromAssembly(string assemblyName)
            {
                criteria.AssemblyNames.Add(assemblyName);
                return this;
            }

            public IEventQuery FromNamespace(string @namespace)
            {
                criteria.Namespaces.Add(@namespace);
                return this;
            }

            public IEventQuery FromSource(string sourceType)
            {
                criteria.SourceTypes.Add(sourceType);
                return this;
            }

            public IEventQuery Until(DateTime endDate)
            {
                criteria.EndDate = endDate;
                return this;
            }
        }

    }
}
