using System.CommandLine;
using Agentsmd.Commands;

namespace Agentsmd;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var root = new RootCommand("agentsmd — AI Agent workflow manager")
        {
            CommandFactory.CreateAgentCommand(),
            CommandFactory.CreateSkillCommand(),
            CommandFactory.CreateWorkflowCommand(),
            CommandFactory.CreateSearchCommand(),
            CommandFactory.CreateListCommand(),
            CommandFactory.CreateSyncCommand(),
            CommandFactory.CreateStatusCommand(),
            CommandFactory.CreateInitCommand()
        };

        root.AddGlobalOption(CommandFactory.ProjectOption);

        return await root.InvokeAsync(args);
    }
}
