using Microsoft.Extensions.Configuration;
using Model;
using SvodBot;
using SvodBot.Interfaces;
using SvodBot.Models;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false);

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
