using System;

namespace CQRS.Infrastructure.Database
{
    public interface IAggregateRoot
    {
        Guid Id { get; }
    }
}
