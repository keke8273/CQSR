﻿using CQRS.Infrastructure.MessageLog;
using CQRS.Infrastructure.Messaging;
using CQRS.Infrastructure.Misc;
using CQRS.Infrastructure.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.Sql.MessageLog
{
    public class SqlMessageLog : IEventLogReader
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
            using(var context = new MessageLogDbContext(this.nameOrConnectionString))
	        {
                var metadata = metadataProvider.GetMetadata(@event);
                context.Set<MessageLogEntity>().Add(new MessageLogEntity
                    {
                        Id = Guid.NewGuid(),
                        SourceId = @event.SourceId.ToString(),
                        Kind = metadata.TryGetValue(StandardMetadata.Kind),
                        AssemblyName = metadata.TryGetValue(StandardMetadata.AssemblyName),
                        FullName = metadata.TryGetValue(StandardMetadata.FullName),
                        Namespace = metadata.TryGetValue(StandardMetadata.Namespace),
                        TypeName = metadata.TryGetValue(StandardMetadata.TypeName),
                        SourceType = metadata.TryGetValue(StandardMetadata.SourceType),
                        CreationDate = DateTime.UtcNow.ToString("o"),
                        Payload = serializer.Serialize(@event),
                    });
                context.SaveChanges();
	        }
        }

        public void Save(ICommand command)
        {
            using (var context = new MessageLogDbContext(this.nameOrConnectionString))
            {
                var metadata = this.metadataProvider.GetMetadata(command);

                context.Set<MessageLogEntity>().Add(new MessageLogEntity
                {
                    Id = Guid.NewGuid(),
                    SourceId = command.Id.ToString(),
                    Kind = metadata.TryGetValue(StandardMetadata.Kind),
                    AssemblyName = metadata.TryGetValue(StandardMetadata.AssemblyName),
                    FullName = metadata.TryGetValue(StandardMetadata.FullName),
                    Namespace = metadata.TryGetValue(StandardMetadata.Namespace),
                    TypeName = metadata.TryGetValue(StandardMetadata.TypeName),
                    SourceType = metadata.TryGetValue(StandardMetadata.SourceType) as string,
                    CreationDate = DateTime.UtcNow.ToString("o"),
                    Payload = serializer.Serialize(command),
                });
                context.SaveChanges();
            }
        }

        public IEnumerable<IEvent> Query(QueryCriteria criteria)
        {
            return new SqlQuery(this.nameOrConnectionString, this.serializer, criteria);
        }

        private class SqlQuery:IEnumerable<IEvent>
        {
            private string nameOrConnectionString;
            private ITextSerializer serializer;
            private QueryCriteria criteria;

            public SqlQuery(string nameOrConnectionString, ITextSerializer serializer, QueryCriteria criteria)
            {
                this.nameOrConnectionString = nameOrConnectionString;
                this.serializer = serializer;
                this.criteria = criteria;
            }
        
            public IEnumerator<IEvent> GetEnumerator()
            {
                return new DisposingEnumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private class DisposingEnumerator : IEnumerator<IEvent>
            {
                private SqlQuery sqlQuery;
                private MessageLogDbContext context;
                private IEnumerator<IEvent> events;

                public DisposingEnumerator (SqlQuery sqlQuery)
	{
                    this.sqlQuery = sqlQuery;
	}

                ~DisposingEnumerator()
                {
                    if(context != null) context.Dispose();
                }

                public void Dispose()
                {
                    if(context != null)
                    {
                        context.Dispose();
                        context = null;
                        GC.SuppressFinalize(this);
                    }

                    if(events != null)
                    {
                        events.Dispose();
                    }
                }

                public IEvent Current{get{return events.Current;}}
                object IEnumerator.Current {get {return this.Current;}}

                public bool MoveNext()
                {
                    if(context == null)
                    {
                        context = new MessageLogDbContext(sqlQuery.nameOrConnectionString);
                        var queryable = context.Set<MessageLogEntity>().AsQueryable()
                            .Where(x => x.Kind == StandardMetadata.EventKind);

                        var where = sqlQuery.criteria.ToExpression();
                        if(where != null)
                            queryable = queryable.Where(where);

                        events = queryable.AsEnumerable().Select(x => this.sqlQuery.serializer.Deserialize<IEvent>(x.Payload))
                            .GetEnumerator();
                    }

                    return events.MoveNext();
                }

                public void Reset()
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}
