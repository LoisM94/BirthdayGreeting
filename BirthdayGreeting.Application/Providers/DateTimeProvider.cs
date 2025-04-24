namespace BirthdayGreeting.Application.Providers;

public class DefaultDateTimeProvider : IDateTimeProvider
{
    public DateTime Today => DateTime.Today;
    public DateTime Now => DateTime.Now;
}