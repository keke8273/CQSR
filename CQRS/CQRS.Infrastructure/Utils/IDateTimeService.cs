using System;

namespace CQRS.Infrastructure.Utils
{
    public interface IDateTimeService
    {
        DateTime GetCurrentDateTimeUtc();
    }
}
