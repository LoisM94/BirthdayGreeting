namespace BirthdayGreeting.Application.Providers;

public interface IDateTimeProvider
{
    DateTime Today { get; }
    DateTime Now { get; }
}