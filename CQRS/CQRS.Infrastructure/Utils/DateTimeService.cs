using System;

namespace CQRS.Infrastructure.Utils
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime GetCurrentDateTimeUtc()
        {
            return DateTime.UtcNow;
        }
    }
}
