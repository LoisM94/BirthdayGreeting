using BirthdayGreeting.Application.Providers;
using BirthdayGreeting.Application.Senders;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BirthdayGreeting.Application.Services;

public class BirthdayGreetingService : IBirthdayGreetingService
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPersonDataProvider _dataProvider;
    private readonly IMessageSender _messageSender;
    private readonly IValidator<Person> _friendValidator;
    private readonly ILogger<BirthdayGreetingService> _logger;

    public BirthdayGreetingService(IPersonDataProvider dataProvider, IMessageSender messageSender, IValidator<Person> friendValidator, ILogger<BirthdayGreetingService> logger, IDateTimeProvider dateTimeProvider)
    {
        _dataProvider = dataProvider;
        _messageSender = messageSender;
        _friendValidator = friendValidator;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task SendBirthdayGreetingsAsync()
    {
        var today = _dateTimeProvider.Today;

        var people = GetFriends(today);

        foreach (var person in people)
        {
            var validationResult = await _friendValidator.ValidateAsync(person);

            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    //consider adding an Id column to the CSV file.  This way we don't have to pass potential sensitive data like name and email to log messages
                    _logger.LogError("Validation failed for {FirstName} {LastName}: {ErrorMessage}.",
                        person.FirstName,
                        person.LastName,
                        error.ErrorMessage);
                }

                continue;
            }

            var response = await _messageSender.SendMessageAsync(person.Email, person.FirstName);

            if (response is { IsSuccessStatusCode: true })
            {
                _logger.LogInformation("Email sent to {Email}", person.Email);
            }
            else
            {
                if (response == null)
                {
                    _logger.LogError(
                        "Failed to send birthday email to {Person}. Response from SendMessageAsync was null",
                        person.Email);
                }
                else
                    _logger.LogError(
                        "Failed to send birthday email to {Person}. Status Code: {StatusCode}. Response Body: {ResponseBody}",
                        person.Email,
                        response.StatusCode,
                        await response.Body.ReadAsStringAsync());
            }
        }
    }

    private IEnumerable<Person> GetFriends(DateTime today)
    {
        IEnumerable<Person> people;
        if (IsFebruary28(today))
        {
            people = _dataProvider.GetPeople().Where(f => f.DateOfBirth?.Month == today.Month && (f.DateOfBirth?.Day == today.Day || f.DateOfBirth?.Day == 29));
        }
        else
        {
            people = _dataProvider.GetPeople()
                .Where(f => f.DateOfBirth?.Month == today.Month && f.DateOfBirth?.Day == today.Day);
        }

        return people;
    }
    private bool IsFebruary28(DateTime dateTime)
    {
        return dateTime is { Month: 2, Day: 28 };
    }
}