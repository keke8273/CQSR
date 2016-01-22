using System;

namespace CQRS.Infrastructure.Utils
{
    public static class DateTimeUtil
    {
        private static IDateTimeService _dateTimeService;

        public static DateTime Now
        {
            get
            {
                if (
                _dateTimeService == null)
                    throw new NullReferenceException("DateTimeService is not registered.");

                return _dateTimeService.GetCurrentDateTimeUtc();    
            }
        }

        public static void SetDateTimeService(IDateTimeService dateTimeService)
        {
            _dateTimeService = dateTimeService;
        }
    }
}
