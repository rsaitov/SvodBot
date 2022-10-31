using SvodBot.Bot;

namespace SvodBot;

/// <summary>
/// Starting class, that executes all the stuff
/// More info on https://siderite.dev/blog/creating-console-app-with-dependency-injection-in-/#at3793184247
/// </summary>
public class DiContainer
{
    public readonly IBot _bot;
    public DiContainer(IBot bot)
    {
        _bot = bot;
    }

    public async Task ExecuteAsync()
    {
        await _bot.StartMessageRecevingAsync();
    }
}