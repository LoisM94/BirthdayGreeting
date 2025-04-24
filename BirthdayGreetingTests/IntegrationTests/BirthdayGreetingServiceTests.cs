using BirthdayGreeting.Application.Providers;
using BirthdayGreeting.Application.Senders;
using BirthdayGreeting.Application.Services;
using BirthdayGreeting.Application.Validators;
using BirthdayGreeting.Tests.IntegrationTests.Fixtures;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using SendGrid;

namespace BirthdayGreeting.Tests.IntegrationTests;

public class BirthdayGreetingServiceIntegrationTests : IClassFixture<SendGridFixture>, IClassFixture<FriendCsvDataProviderFixture>
{
    private readonly SendGridFixture _sendGridFixture;
    private readonly FriendCsvDataProviderFixture _friendFixture;
    private readonly IValidator<Person> _friendValidator;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly Mock<ILogger<BirthdayGreetingService>> _loggerMock;

    public BirthdayGreetingServiceIntegrationTests(SendGridFixture sendGridFixture, FriendCsvDataProviderFixture friendFixture)
    {
        _sendGridFixture = sendGridFixture;
        _friendFixture = friendFixture;

        _friendValidator = new PersonValidator();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(x => x.Today).Returns(new DateTime(2000, 04, 22)); // Matches date time of a Person in the CSV file

        _loggerMock = new Mock<ILogger<BirthdayGreetingService>>();
    }

    [Fact]
    public async Task SendBirthdayGreetingsAsync_Should_SendEmailsSuccessfully_AndLogInformation()
    {
        // Arrange
        _sendGridFixture.ResetToDefaultBehavior();

        // Act
        IMessageSender messageSender = new EmailMessageSender(
            _sendGridFixture.SendGridClient,
            new LoggerFactory().CreateLogger<EmailMessageSender>()
        );

        var birthdayGreetingService = new BirthdayGreetingService(
            _friendFixture.FriendDataProvider,
            messageSender,
            _friendValidator,
            _loggerMock.Object,
            _dateTimeProviderMock.Object
        );

        await birthdayGreetingService.SendBirthdayGreetingsAsync();

        // Assert
        _loggerMock.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) =>
                    @object.ToString()!.Contains("Email sent to ")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task SendBirthdayGreetingsAsync_WhenEmailSendingFails_ShouldLogError()
    {
        // Arrange
        _sendGridFixture.ConfigureClientBehavior(_ =>
            new Response(System.Net.HttpStatusCode.InternalServerError, new StringContent("Error"), null)
        );
        
        IMessageSender messageSender = new EmailMessageSender(
            _sendGridFixture.SendGridClient,
            new LoggerFactory().CreateLogger<EmailMessageSender>()
        );

        var birthdayGreetingService = new BirthdayGreetingService(
            _friendFixture.FriendDataProvider,
            messageSender,
            _friendValidator,
            _loggerMock.Object,
            _dateTimeProviderMock.Object
        );

        // Act
        await birthdayGreetingService.SendBirthdayGreetingsAsync();

        // Assert
        _loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) =>
                    @object.ToString()!.Contains("Failed to send birthday email to")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }
}