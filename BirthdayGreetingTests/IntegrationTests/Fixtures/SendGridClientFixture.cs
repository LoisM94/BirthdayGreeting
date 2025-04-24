using System.Net.Http.Headers;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BirthdayGreeting.Tests.IntegrationTests.Fixtures;

public class SendGridFixture
{
    public ISendGridClient SendGridClient { get; private set; }

    public SendGridFixture()
    {
        SendGridClient = CreateFakeSendGridClient(_ =>
            new Response(System.Net.HttpStatusCode.OK, new StringContent("Success"), null)
        );
    }

    public void ResetToDefaultBehavior()
    {
        ConfigureClientBehavior(_ =>
            new Response(System.Net.HttpStatusCode.OK, new StringContent("Success"), null)
        );
    }
    public void ConfigureClientBehavior(Func<SendGridMessage, Response> behavior)
    {
        SendGridClient = CreateFakeSendGridClient(behavior);
    }

    private ISendGridClient CreateFakeSendGridClient(Func<SendGridMessage, Response> behavior)
    {
        return new FakeSendGridClient(behavior);
    }

    private class FakeSendGridClient : ISendGridClient
    {
        private readonly Func<SendGridMessage, Response> _behavior;

        public FakeSendGridClient(Func<SendGridMessage, Response> behavior)
        {
            _behavior = behavior;
        }

        public Task<Response> SendEmailAsync(SendGridMessage msg, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_behavior(msg));
        }

        //We do not need to implement the rest of the ISendGridClient methods for this test
        public AuthenticationHeaderValue AddAuthorization(KeyValuePair<string, string> header)
        {
            throw new NotImplementedException();
        }

        public Task<Response> MakeRequest(HttpRequestMessage request, CancellationToken cancellationToken = new())
        {
            throw new NotImplementedException();
        }

        public Task<Response> RequestAsync(BaseClient.Method method, string requestBody = null, string queryParams = null, string urlPath = null,
            CancellationToken cancellationToken = new())
        {
            throw new NotImplementedException();
        }

        public string UrlPath { get; set; }
        public string Version { get; set; }
        public string MediaType { get; set; }
    }
}