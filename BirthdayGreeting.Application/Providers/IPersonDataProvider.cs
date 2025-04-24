using BirthdayGreeting.Application.Services;

namespace BirthdayGreeting.Application.Providers;

public interface IPersonDataProvider
{
    public IEnumerable<Person> GetPeople();
}