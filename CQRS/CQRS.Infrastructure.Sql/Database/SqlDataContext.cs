using System;
using System.Data.Entity;
using CQRS.Infrastructure.Database;
using CQRS.Infrastructure.Messaging;

namespace CQRS.Infrastructure.Sql.Database
{
    public class SqlDataContext<T> : IDataContext<T> where T: class, IAggregateRoot
    {
        private readonly IEventBus _eventBus;
        private readonly DbContext _context;

        public SqlDataContext(Func<DbContext> contextFactory, IEventBus eventBus)
        {
            _eventBus = eventBus;
            _context = contextFactory.Invoke();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SqlDataContext()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
        }

        public T Find(Guid id)
        {
            return this._context.Set<T>().Find(id);
        }

        public void Save(T aggregate)
        {
            var entry = _context.Entry(aggregate);

            if (entry.State == EntityState.Detached)
                _context.Set<T>().Add(aggregate);

            _context.SaveChanges();

            var eventPublisher = aggregate as IEventPublisher;
            if(eventPublisher != null)
                _eventBus.Publish(eventPublisher.Events);
        }
    }
}
