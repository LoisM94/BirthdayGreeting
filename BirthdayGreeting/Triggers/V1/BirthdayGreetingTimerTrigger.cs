using BirthdayGreeting.Application.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BirthdayGreeting.Triggers.V1
{
    public class BirthdayGreetingTimerTrigger
    {
        private readonly ILogger _logger;
        private readonly IBirthdayGreetingService _birthdayGreetingService;

        public BirthdayGreetingTimerTrigger(ILoggerFactory loggerFactory, IBirthdayGreetingService birthdayGreetingService)
        {
            _birthdayGreetingService = birthdayGreetingService;
            _logger = loggerFactory.CreateLogger<BirthdayGreetingTimerTrigger>();
        }

        [Function("BirthdayGreeting")]
        public async Task Run([TimerTrigger("0 0 8 * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"Birthday Greeting Timer trigger function executed at: {DateTime.Now}");

            // currently there isn't a way to check the success/fail of the service without manually checking the logs.  Consider handling a response from the Service, and set up an alert/email to host if the response is not successful.
            await _birthdayGreetingService.SendBirthdayGreetingsAsync();
            
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
