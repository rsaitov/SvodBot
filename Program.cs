using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SvodBot;
using SvodBot.Bot;
using SvodBot.Executor;
using SvodBot.Models;

// Host Based Console App
// https://thecodeblogger.com/2021/05/11/how-to-enable-logging-in-net-console-applications/

var host = Host.CreateDefaultBuilder(args)
.ConfigureAppConfiguration(config =>
{
    config.AddJsonFile("appsettings.json", optional: false);
    config.AddJsonFile("appsettings.Development.json", optional: true);
})
.ConfigureServices((context, services) =>
{
    ConfigureCustomServices(services, context.Configuration);
})
.ConfigureLogging((context, logging) =>
{
    logging.ClearProviders();
    logging.AddConfiguration(context.Configuration.GetSection("Logging"));
    logging.AddSimpleConsole(options => options.IncludeScopes = true);
})
.Build();

var workerInstance = host.Services.GetRequiredService<DiContainer>();
await workerInstance.ExecuteAsync();

static void ConfigureCustomServices(IServiceCollection services, IConfiguration config)
{
    var telegramConfiguration = config
        .GetSection("TelegramConfiguration")
        .Get<TelegramConfiguration>();
    var executorConfiguration = config
        .GetSection("ExecutorConfiguration")
        .Get<ExecutorConfiguration>();

    services
        .AddTransient<DiContainer>()
        .AddSingleton(telegramConfiguration)
        .AddSingleton(executorConfiguration)
        .AddSingleton<IExecutor, BatExecutor>()
        .AddSingleton<IBot, TelegramBot>()
        .AddSingleton<DiContainer>();
}