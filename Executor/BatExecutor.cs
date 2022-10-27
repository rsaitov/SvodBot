using SvodBot.Models;

namespace SvodBot.Executor;

public class BatExecutor : IExecutor
{
    private readonly ExecutorConfiguration _executorConfiguration;

    public BatExecutor(ExecutorConfiguration executorConfiguration)
    {
        _executorConfiguration = executorConfiguration;
    }
    public async Task StartExeAsync(string parameter)
    {
        await Task.Delay(0);
        Console.WriteLine($"--> Executing: {_executorConfiguration.FileName} with argument: {parameter}");

        System.Diagnostics.Process.Start(_executorConfiguration.FileName, parameter);
    }
}