using System;
using Microsoft.Extensions.CommandLineUtils;

namespace DataEntry.Cli
{
    internal class SequenceUploadCommand : CommandBase
    {
        private CommandArgument _filePath;
        private CommandOption _dryRun;
        private CommandOption _stopOnError;
        private CommandOption _truncate;
        private CommandArgument _baseUrl;
        private CommandOption _output;
        private CommandOption _debug;
        private CommandOption _clientId;
        private CommandOption _clientSecret;

        public override void Register(CommandLineApplication app)
        {
            app.Command("sequence-upload", (cmd) => Configure(cmd));
        }

        protected override void DefineArgumentsAndOptions(CommandLineApplication cmd)
        {
            cmd.Description = "Upload sequences results to data entry";

            _filePath = cmd.Argument(
                "<RESULTS>",
                "Path file to results.");

            _baseUrl = cmd.Argument(
                "<BASE URL>",
                "TODO");

            _dryRun = cmd.Option(
                "-n |--dryrun",
                "Do nothing; only show what would happen",
                CommandOptionType.NoValue);

            _stopOnError = cmd.Option(
                "-s | --stop-on-error",
                "TODO",
                CommandOptionType.NoValue);

            _truncate = cmd.Option(
                "-t | --truncate",
                "TODO",
                CommandOptionType.NoValue);

            _output = cmd.Option(
                "-o | --output",
                "TODO",
                CommandOptionType.SingleValue);

            _debug = cmd.Option(
                "--debug",
                "TODO",
                CommandOptionType.NoValue);

            _clientId = cmd.Option(
                "--client-id",
                "TODO",
                CommandOptionType.SingleValue);

            _clientSecret = cmd.Option(
                "--client-secret",
                "TODO",
                CommandOptionType.SingleValue);


            base.DefineArgumentsAndOptions(cmd);
        }

        protected override void Validate()
        {
            if (string.IsNullOrEmpty(_filePath.Value))
            {
                throw new ArgumentNullException(_filePath.Name);
            }

            if (string.IsNullOrEmpty(_baseUrl.Value))
            {
                throw new ArgumentNullException(_baseUrl.Name);
            }

            Uri baseUri;
            if (Uri.TryCreate(_baseUrl.Value, UriKind.Absolute, out baseUri) == false)
            {
                throw new ArgumentException(_baseUrl.Name);
            }

            if (string.IsNullOrEmpty(_clientId.Value()))
            {
                throw new ArgumentNullException(_clientId.LongName);
            }

            if (string.IsNullOrEmpty(_clientSecret.Value()))
            {
                throw new ArgumentNullException(_clientSecret.LongName);
            }
        }
    }
}