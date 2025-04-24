using Microsoft.Extensions.Logging;
using Polly;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BirthdayGreeting.Application.Senders;

public class EmailMessageSender : IMessageSender
{
    private readonly ISendGridClient _sendGridClient;
    private readonly IAsyncPolicy _retryPolicy;

    public EmailMessageSender(ISendGridClient sendGridClient, ILogger<EmailMessageSender> logger)
    {
        _sendGridClient = sendGridClient;
        _retryPolicy = PollyPolicies.CreateRetryPolicy(logger);
    }

    public async Task<Response?> SendMessageAsync(string recipient, string firstName)
    {
        Response response = null!;
        await _retryPolicy.ExecuteAsync(async () =>
        {
            var sendGridMessage = new SendGridMessage
            {
                //consider fetching email address from a secure location like key vault if deploying to a cloud service
                From = new EmailAddress(Environment.GetEnvironmentVariable("FromEmailAddress")),
                Subject = "Happy birthday!",
                PlainTextContent = $"Happy birthday, dear {firstName}!"
            };
            sendGridMessage.AddTo(recipient);

            response = await _sendGridClient.SendEmailAsync(sendGridMessage);
        });

        return response;
    }
}