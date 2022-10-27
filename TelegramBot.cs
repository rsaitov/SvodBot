using SvodBot.Interfaces;
using SvodBot.Models;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Model;

public class TelegramBot : IBot
{
    private readonly TelegramBotClient _client;
    private readonly IExecutor _executor;

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
        // Only process Message updates: https://core.telegram.org/bots/api#message
        if (update.Message is not { } message)
            return;
        // Only process text messages
        if (message.Text is not { } messageText)
            return;

        var chatId = message.Chat.Id;

        if (DateTime.TryParse(messageText, out var svodDate))
        {
            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
            await _executor.StartExeAsync(messageText);

            var sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Date report received:\n" + messageText,
                cancellationToken: cancellationToken);
        }
        else
        {
            var sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Date parsing ERROR:\n" + messageText,
                cancellationToken: cancellationToken);
        }
    }

    Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
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