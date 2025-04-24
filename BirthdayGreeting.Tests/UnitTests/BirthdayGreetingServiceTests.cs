using System.Net;
using BirthdayGreeting.Application.Senders;
using BirthdayGreeting.Application.Providers;
using BirthdayGreeting.Application.Services;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using SendGrid;

namespace BirthdayGreeting.Tests.UnitTests;

public class BirthdayGreetingServiceTests
{
    private readonly Mock<IPersonDataProvider> _friendDataProviderMock = new();
    private readonly Mock<IMessageSender> _messageSenderMock = new();
    private readonly Mock<IValidator<Person>> _friendValidatorMock = new();
    private readonly Mock<ILogger<BirthdayGreetingService>> _loggerMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly IBirthdayGreetingService _birthdayGreetingService;

    public BirthdayGreetingServiceTests()
    {
        _birthdayGreetingService = new BirthdayGreetingService(
            _friendDataProviderMock.Object,
            _messageSenderMock.Object,
            _friendValidatorMock.Object,
            _loggerMock.Object,
            _dateTimeProviderMock.Object);
    }
    [Fact]
    public async Task SendBirthdayGreetingsAsync_When_ValidationFails_Then_DoNotSendMessages()
    {
        var people = new List<Person>
        {
            new()
            {
                DateOfBirth = new DateTime(2025, 04, 22),
                Email = "johndoe@example.com",
                FirstName = "John",
                LastName = "Doe"
            }
        };

        _friendDataProviderMock
            .Setup(x => x.GetPeople())
            .Returns(people);

        _dateTimeProviderMock
            .Setup(x => x.Today)
            .Returns(new DateTime(2025, 04, 22));

        _friendValidatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[] { new FluentValidation.Results.ValidationFailure("Email", "Invalid email") }));

        // Act
        await _birthdayGreetingService.SendBirthdayGreetingsAsync();

        // Assert
        _friendDataProviderMock.Verify(x => x.GetPeople(), Times.Once);
        _friendValidatorMock.Verify(x => x.ValidateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Once);
        _messageSenderMock.Verify(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendBirthdayGreetingsAsync_When_TodayIsFebruary28_And_FriendIncludesBirthdayOnLeapDayAndToday_Then_SendMessages_And_LogInformation()
    {
        // Arrange
        var people = new List<Person>
        {
            // Person was born on Leap Day
            new()
            {
                DateOfBirth = new DateTime(1996, 02, 29),
                Email = "foobar@example.com",
                FirstName = "Foo",
                LastName = "Bar"
            },
            
            // Person was born on February 28
            new()
            {
                DateOfBirth = new DateTime(1996, 02, 28),
                Email = "foobar2@example.com",
                FirstName = "Foo2",
                LastName = "Bar2"
            }
        };

        _friendDataProviderMock
            .Setup(x => x.GetPeople())
            .Returns(people);

        // today is February 28
        _dateTimeProviderMock
            .Setup(x => x.Today)
            .Returns(new DateTime(2025, 02, 28));

        _friendValidatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _messageSenderMock
            .Setup(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Response(HttpStatusCode.OK, new StringContent("Success"), null));

        // Act
        await _birthdayGreetingService.SendBirthdayGreetingsAsync();

        // Assert
        _friendDataProviderMock.Verify(x => x.GetPeople(), Times.Once);
        _friendValidatorMock.Verify(x => x.ValidateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _messageSenderMock.Verify(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));

        _loggerMock.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) =>
                    @object.ToString()!.Contains("Email sent to ")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(2));
    }


    [Fact]
    public async Task SendBirthdayGreetingsAsync_When_SendMessage_ReturnsNull_DoNotSendMessages_AndLogError()
    {
        // Arrange
        _dateTimeProviderMock
            .Setup(x => x.Today)
            .Returns(new DateTime(2025, 02, 01));

        var people = new List<Person>
        {
            new()
            {
                DateOfBirth = new DateTime(1996, 02, 01),
                Email = "foobar@example.com",
                FirstName = "Foo",
                LastName = "Bar"
            }
        };

        _friendDataProviderMock
            .Setup(x => x.GetPeople())
            .Returns(people);

        _friendValidatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _messageSenderMock
            .Setup(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(() => null!);

        // Act
        await _birthdayGreetingService.SendBirthdayGreetingsAsync();

        // Assert
        _friendDataProviderMock.Verify(x => x.GetPeople(), Times.Once);
        _friendValidatorMock.Verify(x => x.ValidateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Once);
        _messageSenderMock.Verify(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        _loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) =>
                    @object.ToString() == "Failed to send birthday email to foobar@example.com. Response from SendMessageAsync was null"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }


    [Fact]
    public async Task SendBirthdayGreetingsAsync_When_MultipleFriendsHaveBirthday_Then_SendMessages_And_LogInformation()
    {
        // Arrange
        var people = new List<Person>
        {
            new()
            {
                DateOfBirth = new DateTime(1996, 05, 05),
                Email = "foobar@example.com",
                FirstName = "Foo",
                LastName = "Bar"
            },
            new()
            {
                DateOfBirth = new DateTime(1996, 05, 05),
                Email = "foobar2@example.com",
                FirstName = "Foo2",
                LastName = "Bar2"
            }
        };

        _friendDataProviderMock
            .Setup(x => x.GetPeople())
            .Returns(people);

        // today is May 5, 2024
        _dateTimeProviderMock
            .Setup(x => x.Today)
            .Returns(new DateTime(2024, 05, 5));

        _friendValidatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _messageSenderMock
            .Setup(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Response(HttpStatusCode.OK, new StringContent("Success"), null));

        // Act
        await _birthdayGreetingService.SendBirthdayGreetingsAsync();

        // Assert
        _friendDataProviderMock.Verify(x => x.GetPeople(), Times.Once);
        _friendValidatorMock.Verify(x => x.ValidateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _messageSenderMock.Verify(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));

        _loggerMock.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) =>
                    @object.ToString()!.Contains("Email sent to ")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task SendBirthdayGreetingsAsync_When_NoFriendsHaveBirthday_Then_DoNotSendMessages()
    {
        // Arrange
        var people = new List<Person>
        {
            // Person was born on February 27
            new()
            {
                DateOfBirth = new DateTime(1996, 02, 27),
                Email = "foobar@example.com",
                FirstName = "Foo",
                LastName = "Bar"
            }
        };

        _friendDataProviderMock
            .Setup(x => x.GetPeople())
            .Returns(people);

        // today is February 28
        _dateTimeProviderMock
            .Setup(x => x.Today)
            .Returns(new DateTime(2025, 02, 28));

        _friendValidatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _messageSenderMock
            .Setup(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Response(HttpStatusCode.OK, new StringContent("Success"), null));

        // Act
        await _birthdayGreetingService.SendBirthdayGreetingsAsync();

        // Assert
        _friendDataProviderMock.Verify(x => x.GetPeople(), Times.Once);
        _friendValidatorMock.Verify(x => x.ValidateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
        _messageSenderMock.Verify(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendBirthdayGreetingsAsync_When_MessageSenderIsNotSuccessfulResponse_Then_LogError()
    {
        // Arrange

        var people = new List<Person>
        {
            new()
            {
                DateOfBirth = new DateTime(2025, 04, 22),
                Email = "johndoe@example.com",
                FirstName = "John",
                LastName = "Doe"
            }
        };

        _friendDataProviderMock
            .Setup(x => x.GetPeople())
            .Returns(people);

        _dateTimeProviderMock
            .Setup(x => x.Today)
            .Returns(new DateTime(2025, 04, 22));

        _friendValidatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _messageSenderMock
            .Setup(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Response(HttpStatusCode.InternalServerError, new StringContent("Error"), null));

        // Act
        await _birthdayGreetingService.SendBirthdayGreetingsAsync();

        // Assert
        _loggerMock.Verify(logger => logger.Log(
        It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
        It.Is<EventId>(eventId => eventId.Id == 0),
        It.Is<It.IsAnyType>((@object, @type) =>
            @object.ToString() == "Failed to send birthday email to johndoe@example.com. Status Code: InternalServerError. Response Body: Error"),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
    }
}