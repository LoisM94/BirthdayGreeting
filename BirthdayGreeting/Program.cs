using BirthdayGreeting.Application.Validators;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SendGrid;
using BirthdayGreeting.Application.Providers;
using BirthdayGreeting.Application.Senders;
using BirthdayGreeting.Application.Services;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureAppConfiguration((_, config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureFunctionsWebApplication()
    .ConfigureLogging(logging =>
    {
        logging.AddConsole(); 
        logging.SetMinimumLevel(LogLevel.Debug);
    })
    .ConfigureServices((hostingContext, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // consider using a secure location like key vault to inject secret if deploying to a cloud service
        var sendGridApiKey = Environment.GetEnvironmentVariable("SendGridApiKey");
        services.AddSingleton<ISendGridClient>(_ => new SendGridClient(sendGridApiKey));

        var csvFilePath = hostingContext.Configuration["CsvSettings:FilePath"];
        services.AddSingleton<IPersonDataProvider>(sp => new PersonCsvDataProvider(csvFilePath!,
                sp.GetRequiredService<ILogger<PersonCsvDataProvider>>()));

        services.AddTransient<IValidator<Person>, PersonValidator>();
        services.AddTransient<IDateTimeProvider, DefaultDateTimeProvider>();
        services.AddSingleton<IMessageSender, EmailMessageSender>();
        services.AddSingleton<IBirthdayGreetingService, BirthdayGreetingService>();
    })
    .Build();

host.Run();
