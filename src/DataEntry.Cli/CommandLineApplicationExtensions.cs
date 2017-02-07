using Microsoft.Extensions.CommandLineUtils;

namespace DataEntry.Cli
{
    internal static class CommandLineApplicationExtensions
    {
        public static CommandLineApplication RegisterCommand<TCommand>(this CommandLineApplication app)
            where TCommand : CommandBase, new()
        {
            var command = new TCommand();
            command.Register(app);
            return app;
        }
    }
}