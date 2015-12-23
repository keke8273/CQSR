using CQRS.Infrastructure.MessageLog;
using CQRS.Infrastructure.Messaging;
using CQRS.Infrastructure.Misc;
using CQRS.Infrastructure.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.Sql.MessageLog
{
    public class SqlMessageLog ： IEventLogReader
    {
        private string nameOrConnectionString;
        private IMetadataProvider metadataProvider;
        private ITextSerializer serializer;

        public SqlMessageLog (string nameOrConnectionString, ITextSerializer serializer, IMetadataProvider metadataProvider)
        {
            this.nameOrConnectionString = nameOrConnectionString;
            this.serializer = serializer;
            this.metadataProvider = metadataProvider;
        }

        public void Save(IEvent @event)
        {

        }

        public void Save(ICommand command)
        {

        }

        public IEnumerable<IEvent> Query()
    }
}
