using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
