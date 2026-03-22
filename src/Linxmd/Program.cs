using System.CommandLine;
using Linxmd.Commands;
using Linxmd.Services;

namespace Linxmd;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var updateTask = UpdateNotifier.TryNotifyAsync();

        var root = new RootCommand("linxmd — AI Agent workflow manager")
        {
            CommandFactory.CreateAddCommand(),
            CommandFactory.CreateRemoveCommand(),
            CommandFactory.CreateListCommand(),
            CommandFactory.CreateSyncCommand(),
            CommandFactory.CreateStatusCommand(),
            CommandFactory.CreateInitCommand(),
            CommandFactory.CreateUpdateCommand()
        };

        root.AddGlobalOption(CommandFactory.ProjectOption);

        var exitCode = await root.InvokeAsync(args);
        await updateTask;
        return exitCode;
    }
}
