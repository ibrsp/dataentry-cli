﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.FileSystemGlobbing;

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

        protected override int Execute()
        {
            var pattern = _filePath.Value;
            var pathRoot = Path.GetPathRoot(_filePath.Value);
            var isPathRooted = string.IsNullOrWhiteSpace(pathRoot) == false;

            var matchedFiles = new Matcher()
                .AddInclude(isPathRooted
                    ? pattern.Substring(pathRoot.Length)
                    : pattern )
                .GetResultsInFullPath(isPathRooted
                    ? pathRoot
                    : Directory.GetCurrentDirectory())
                .ToList();


            if (matchedFiles.Any() == false)
            {
                throw new InvalidOperationException();
            }

            var payload = matchedFiles
                .SelectMany(fn => ParseSequencePayload(File.OpenRead(fn)))
                .ToList();

            return base.Execute();
        }

        private IEnumerable<SequenceRequestPayload> ParseSequencePayload(Stream data)
        {
            using (var reader = new CsvReader(new StreamReader(data)))
            {
                while (reader.Read())
                {
                    yield return new SequenceRequestPayload
                    {
                        OrganizationIdentifier = reader.GetField<string>(0),
                        PatientIdentifier = reader.GetField<string>(1),
                        SpecimenIdentifier = reader.GetField<string>(2),
                        SpecimenCollectedDate = reader.GetField<string>(3),
                        SourceOrganism = reader.GetField<string>(4),
                        TaxonIdentifier = reader.GetField<string>(5),
                        BioProject = reader.GetField<string>(6),
                        BioSample = reader.GetField<string>(7),
                        SraIdentifiers = reader.GetField<string>(8),
                    };
                }
            }
        }

        private class SequenceRequestPayload
        {
            public string OrganizationIdentifier { get; set; }
            public string PatientIdentifier { get; set; }
            public string SpecimenIdentifier { get; set; }
            public string SpecimenCollectedDate { get; set; }
            public string SourceOrganism { get; set; }
            public string TaxonIdentifier { get; set; }
            public string BioProject { get; set; }
            public string BioSample { get; set; }
            public string SraIdentifiers { get; set; }
        }
    }
}