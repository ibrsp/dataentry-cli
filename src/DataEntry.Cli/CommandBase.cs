using System;
using Microsoft.Extensions.CommandLineUtils;

namespace DataEntry.Cli
{
    internal abstract class CommandBase
    {
        public abstract void Register(CommandLineApplication app);

        protected virtual void DefineArgumentsAndOptions(CommandLineApplication cmd)
        {
            cmd.HelpOption(Constants.CommandOptions.HelpOption);
        }

        protected void Configure(CommandLineApplication cmd)
        {
            DefineArgumentsAndOptions(cmd);

            cmd.OnExecute(() =>
            {
                cmd.ShowRootCommandFullNameAndVersion();
                try
                {
                    Validate();
                }
                catch (ArgumentException ex)
                {
                    throw new CommandParsingException(cmd, ex.Message);
                }

                return Execute();
            });
        }

        protected virtual void Validate()
        {
        }

        protected virtual int Execute()
        {
            return Constants.ExitCodes.Ok;
        }
    }
}