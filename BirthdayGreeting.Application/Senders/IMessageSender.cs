using SendGrid;

namespace BirthdayGreeting.Application.Senders;

public interface IMessageSender
{
    Task<Response?> SendMessageAsync(string recipient, string firstName);
}