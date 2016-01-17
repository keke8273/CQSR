using System;
using System.Collections.Generic;
using CQRS.Infrastructure.Messaging;

namespace CQRS.Infrastructure.Processes
{
    public interface IProcessManager
    {
        Guid Id { get; }

        bool Completed { get; }

        IEnumerable<Envelope<ICommand>> Commands { get; }
    }
}
