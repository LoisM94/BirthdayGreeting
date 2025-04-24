using System.Net;
using BirthdayGreeting.Application.Senders;
using Microsoft.Extensions.Logging;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BirthdayGreeting.Tests.UnitTests;

public class EmailMessageSenderTests
{
    private readonly Mock<ISendGridClient> _sendGridClientMock = new();
    private readonly Mock<ILogger<EmailMessageSender>> _loggerMock = new();
    private readonly IMessageSender _messageSender;

    public EmailMessageSenderTests()
    {
        _sendGridClientMock
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Response(HttpStatusCode.OK, new StringContent("Success"), null));
        _messageSender = new EmailMessageSender(_sendGridClientMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SendMessageAsync_SendEmail_Success()
    {
        // Act
        await _messageSender.SendMessageAsync("example@email.com", "name");

        // Assert
        _sendGridClientMock.Verify(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}