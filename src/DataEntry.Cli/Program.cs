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


            app.OnExecute(() =>
            {
                app.ShowHelp();
                return Constants.ExitCodes.Ok;
            });

            try
            {
                return app.Execute(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);

                (ex as CommandParsingException)?.Command.ShowHelp();
            }

            return Constants.ExitCodes.Bad;
        }
    }
}