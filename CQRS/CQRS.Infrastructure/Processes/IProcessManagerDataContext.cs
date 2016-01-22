using System;
using System.Linq.Expressions;

namespace CQRS.Infrastructure.Processes
{
    public interface IProcessManagerDataContext <T> : IDisposable
        where T : class, IProcessManager
    {
        T Find(Guid id);

        void Save(T processManager);

        T Find(Expression<Func<T, bool>> predicate, bool includeCompleted = false);
    }
}
