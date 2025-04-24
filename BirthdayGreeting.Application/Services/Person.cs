using CsvHelper.Configuration.Attributes;

namespace BirthdayGreeting.Application.Services;

public class Person
{
    [Name("first_name")]
    public string FirstName { get; set; }

    [Name("last_name")]
    public string LastName { get; set; }

    [Name("email")]
    public string Email { get; set; }

    [Name("date_of_birth")]
    public DateTime? DateOfBirth { get; set; }
}