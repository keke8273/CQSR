using CQRS.Infrastructure.Messaging;
using CQRS.Infrastructure.Processes;
using CQRS.Infrastructure.Serialization;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Infrastructure.Sql.Processes
{
    public class SqlProcessManagerDataContext<T> : IProcessManagerDataContext<T> where T : class, IProcessManager
    {
        private readonly ICommandBus commandBus;
        private readonly DbContext context;
        private readonly ITextSerializer serializer;

        public SqlProcessManagerDataContext(Func<DbContext> contextFactory, ICommandBus commandBus, ITextSerializer serializer)
        {
            this.commandBus = commandBus;
            context = contextFactory.Invoke();
            this.serializer = serializer;

        }

        public T Find(Guid id)
        {
            return Find(pm => pm.Id == id, true);
        }

        public T Find(System.Linq.Expressions.Expression<Func<T, bool>> predicate, bool includeCompleted = false)
        {
            T pm = null;

            if(!includeCompleted)
            {
                pm = this.context.Set<T>().Where(predicate.And(x => x.Completed == false)).FirstOrDefault();
            }

            if (pm == null)
            {
                pm = this.context.Set<T>().Where(predicate).FirstOrDefault();
            }

            if (pm != null)
            {
                // TODO: ideally this could be improved to avoid 2 roundtrips to the server.
                var undispatchedMessages = this.context.Set<UndispatchedMessages>().Find(pm.Id);
                try
                {
                    this.DispatchMessages(undispatchedMessages);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // if another thread already dispatched the messages, ignore
                    Trace.TraceWarning("Concurrency exception while marking commands as dispatched for process manager with ID {0} in Find method.", pm.Id);

                    this.context.Entry(undispatchedMessages).Reload();

                    undispatchedMessages = this.context.Set<UndispatchedMessages>().Find(pm.Id);

                    // undispatchedMessages should be null, as we do not have a rowguid to do optimistic locking, other than when the row is deleted.
                    // Nevertheless, we try dispatching just in case the DB schema is changed to provide optimistic locking.
                    this.DispatchMessages(undispatchedMessages);
                }

                if (!pm.Completed || includeCompleted)
                {
                    return pm;
                }
            }

            return null;
        }

        public void Save(T processManager)
        {
            var entry = this.context.Entry(processManager);

            if (entry.State == EntityState.Detached)
                context.Set<T>().Add(processManager);

            var commands = processManager.Commands.ToList();
            UndispatchedMessages undispatched = null;

            if (commands.Count > 0)
            {
                undispatched = new UndispatchedMessages(processManager.Id)
                {
                    Commands = serializer.Serialize(commands)
                };

                this.context.Set<UndispatchedMessages>().Add(undispatched);
            }

            try
            {
                this.context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException e)
            {
                throw new ConcurrencyException(e.Message, e);
            }

            try
            {
                this.DispatchMessages(undispatched, commands);
            }
            catch (DbUpdateConcurrencyException)
            {
                //if another thread already dispatched the messages, ignore.
                Trace.TraceWarning("Ignoring concurrency exception while marking commands as dispatched for process manager with ID {0} in Save method.", processManager.Id);
            }
        }

        private void DispatchMessages(UndispatchedMessages undispatched, List<Envelope<ICommand>> deserializedCommands = null)
        {
            if(undispatched != null)
            {
                if(deserializedCommands == null)
                {
                    deserializedCommands = serializer.Deserialize<IEnumerable<Envelope<ICommand>>>(undispatched.Commands).ToList();
                }

                var originalCommandsCount = deserializedCommands.Count;

                try
                {
                    while (deserializedCommands.Count > 0)
                    {
                        commandBus.Send(deserializedCommands.First());
                        deserializedCommands.RemoveAt(0);
                    }
                }
                catch (Exception)
                {
                    if (originalCommandsCount != deserializedCommands.Count)
                    {
                        undispatched.Commands = this.serializer.Serialize(deserializedCommands);

                        try
                        {
                            this.context.SaveChanges();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            // if another thread already dispatched the messages, ignore and surface original exception instead.
                        }
                    }

                    throw;
                }

                this.context.Set<UndispatchedMessages>().Remove(undispatched);
                context.SaveChanges();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SqlProcessManagerDataContext()
        {
            this.Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                //release all managed resources.
                this.context.Dispose();
            }

            //release all unmanaged resources. 
        }
    }
}
