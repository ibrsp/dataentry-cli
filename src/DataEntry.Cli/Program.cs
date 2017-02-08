using System;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;

namespace DataEntry.Cli
{
    internal class Program
    {
        private Program()
        {
        }

        private static int Main(string[] args)
        {
            var program = new Program();
            return program.Run(args);
        }

        private int Run(string[] args)
        {
            var app = new CommandLineApplication(false)
            {
                Name = "de",
                Description = "Set of tools to autome routine tasks",
                FullName = "data entry toolbox"
            };

            app.HelpOption(Constants.CommandOptions.HelpOption);
            app.VersionOption("--version",
                () => typeof(Program)
                    .GetTypeInfo()
                    .Assembly
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion);

            app
                .RegisterCommand<SequenceUploadCommand>()
                .OnExecute(() =>
                {
                    app.ShowHelp();
                    return Constants.ExitCodes.Ok;
                });

            var reporter = new Reporter(false);
            try
            {
                return app.Execute(args);
            }
            catch (AggregateException ex)
            {
                var flattenedAggregateException = ex.Flatten();

                foreach (var innerException in flattenedAggregateException.InnerExceptions)
                {
                    reporter.WriteError(innerException.Message);
                }

            }
            catch (CommandParsingException ex)
            {
                reporter.WriteError(ex.Message);
                ex.Command.ShowHelp();
            }
            catch (Exception ex)
            {
                reporter.WriteError(ex.Message);
                reporter.WriteError("An unexpected error occurred");
            }

            return Constants.ExitCodes.Bad;
        }
    }
}