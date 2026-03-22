using System.CommandLine;
using Agentsmd.Commands;

namespace Agentsmd;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var root = new RootCommand("agentsmd — AI Agent workflow manager")
        {
            CommandFactory.CreateAddCommand(),
            CommandFactory.CreateRemoveCommand(),
            CommandFactory.CreateListCommand(),
            CommandFactory.CreateSyncCommand(),
            CommandFactory.CreateStatusCommand(),
            CommandFactory.CreateInitCommand(),
            CommandFactory.CreateUpdateCommand(),
            // Deprecated — still functional, print hint
            CommandFactory.CreateAgentCommand(),
            CommandFactory.CreateSkillCommand(),
            CommandFactory.CreateWorkflowCommand()
        };

        root.AddGlobalOption(CommandFactory.ProjectOption);

        return await root.InvokeAsync(args);
    }
}
