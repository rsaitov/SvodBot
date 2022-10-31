using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SvodBot.Bot;
using SvodBot.Executor;
using SvodBot.Models;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Development.json", optional: true);

var services = new ServiceCollection();

IConfiguration config = builder.Build();

var telegramConfiguration = config
    .GetSection("TelegramConfiguration")
    .Get<TelegramConfiguration>();
var executorConfiguration = config
    .GetSection("ExecutorConfiguration")
    .Get<ExecutorConfiguration>();

IExecutor executor = new BatExecutor(executorConfiguration);
IBot bot = new TelegramBot(telegramConfiguration, executor);
await bot.StartMessageRecevingAsync();
