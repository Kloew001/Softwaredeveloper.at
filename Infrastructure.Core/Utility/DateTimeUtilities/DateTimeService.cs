namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility.DateTimeUtilities
{
    public interface IDateTimeService
    {
        DateTime Now();
    }

    public class DateTimeService : IDateTimeService
    {
        public DateTime Now()
        {
            return DateTime.Now;
        }
    }
}
