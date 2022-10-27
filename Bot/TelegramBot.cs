using System.Text.RegularExpressions;
using SvodBot.Executor;
using SvodBot.Models;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SvodBot.Bot;

public class TelegramBot : IBot
{
    private readonly TelegramBotClient _client;
    private readonly IExecutor _executor;
    private readonly Regex regexCommands = new Regex(@"before(\w)day");

    public TelegramBot(
        TelegramConfiguration telegramConfiguration,
        IExecutor executor)
    {
        _client = new TelegramBotClient(telegramConfiguration.Token);
        _executor = executor;
    }

    public async Task StartMessageRecevingAsync()
    {
        using var cts = new CancellationTokenSource();

        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };

        _client.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        var me = await _client.GetMeAsync();

        Console.WriteLine($"Start listening for @{me.Username}");
        Console.ReadLine();

        cts.Cancel();
    }

    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
            return;
        if (message.Text is not { } messageText)
            return;

        var chatId = message.Chat.Id;

        if (DateTime.TryParse(messageText, out _) || regexCommands.IsMatch(messageText))
        {
            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

            if (regexCommands.IsMatch(messageText))
            {
                await HandleMenuCommand(botClient, messageText, chatId, cancellationToken);
            }
            else
            {
                await HandleDateMessage(botClient, messageText, chatId, cancellationToken);
            }
        }
        else
        {
            await HandleMessageError(botClient, messageText, chatId, cancellationToken);
        }
    }

    private async Task HandleMenuCommand(
        ITelegramBotClient botClient,
        string messageText,
        long chatId,
        CancellationToken cancellationToken)
    {
        var match = regexCommands.Match(messageText);
        if (!Int32.TryParse(match.Groups[1].Value, out var daysBefore))
            return;
        var dateFromDaysBefore = DateTime.Now.Date.AddDays(-daysBefore);
        var dateFromDaysBeforeString = dateFromDaysBefore.ToShortDateString();

        await _executor.StartExeAsync(dateFromDaysBeforeString);

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"Command received:\n{messageText}\nDate report: {dateFromDaysBeforeString}",
            cancellationToken: cancellationToken);
    }

    private async Task HandleDateMessage(
        ITelegramBotClient botClient,
        string messageText,
        long chatId,
        CancellationToken cancellationToken)
    {
        await _executor.StartExeAsync(messageText);

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"Date report received:\n{messageText}",
            cancellationToken: cancellationToken);
    }

    private static async Task HandleMessageError(
        ITelegramBotClient botClient, 
        string messageText, 
        long chatId, 
        CancellationToken cancellationToken
        )
    {
        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Date parsing ERROR:\n" + messageText,
            cancellationToken: cancellationToken);
    }

    private Task HandlePollingErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}