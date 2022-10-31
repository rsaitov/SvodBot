using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SvodBot;
using SvodBot.Bot;
using SvodBot.Executor;
using SvodBot.Models;

var services = new ServiceCollection();
ConfigureServices(services);

var diContainer = services.BuildServiceProvider().GetService<DiContainer>();
await diContainer.ExecuteAsync();

void ConfigureServices(ServiceCollection services)
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false)
        .AddJsonFile("appsettings.Development.json", optional: true);

    IConfiguration config = builder.Build();
    var telegramConfiguration = config
        .GetSection("TelegramConfiguration")
        .Get<TelegramConfiguration>();
    var executorConfiguration = config
        .GetSection("ExecutorConfiguration")
        .Get<ExecutorConfiguration>();
        
    services
        .AddSingleton(telegramConfiguration)
        .AddSingleton(executorConfiguration)
        .AddSingleton<IExecutor, BatExecutor>()
        .AddSingleton<IBot, TelegramBot>()
        .AddSingleton<DiContainer>();
}