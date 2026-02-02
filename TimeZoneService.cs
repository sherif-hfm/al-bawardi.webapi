namespace janaez.webapi
{
    public interface ITimeZoneService
    {
        DateTime GetCurrentArabTime(); // returns current time in Arab Standard Time
        DateTime GetArabTime(DateTime localTime); // converts from server local time to Arab Standard Time
    }

    public class TimeZoneService : ITimeZoneService
    {
        private readonly TimeZoneInfo _arabTimeZone;

        public TimeZoneService()
        {
            _arabTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
        }

        public DateTime GetCurrentArabTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _arabTimeZone);
        }

        public DateTime GetArabTime(DateTime localTime)
        {
            // Assume input is in server local time zone
            if (localTime.Kind == DateTimeKind.Unspecified || localTime.Kind == DateTimeKind.Local)
            {
                var utc = localTime.ToUniversalTime(); // convert to UTC using server's local time zone
                return TimeZoneInfo.ConvertTimeFromUtc(utc, _arabTimeZone); // convert to Arab time
            }

            // If input is already UTC, just convert directly
            return TimeZoneInfo.ConvertTimeFromUtc(localTime, _arabTimeZone);
        }
    }

}
