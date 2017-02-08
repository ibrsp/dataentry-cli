using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.FileSystemGlobbing;

namespace DataEntry.Cli
{
    internal class SequenceUploadCommand : CommandBase
    {
        private CommandArgument _filePath;
        private CommandOption _dryRun;
        private CommandOption _truncate;
        private CommandArgument _baseUrl;
        private CommandOption _output;
        private CommandOption _quiet;
        private CommandOption _clientId;
        private CommandOption _clientSecret;

        public override void Register(CommandLineApplication app)
        {
            app.Command("sequence-upload", (cmd) => Configure(cmd));
        }

        protected override void DefineArgumentsAndOptions(CommandLineApplication cmd)
        {
            cmd.Description = "Upload sequences results to data entry";
            cmd.ExtendedHelpText = @"
Examples:
  de sequence-upload ./data/**/2016/*.csv http://localhost --client-secret YOUR_CLIENT_SECRET --client-id YOUR_CLIENT_ID -o ./report.csv  
";
            _filePath = cmd.Argument(
                "<FILENAME>",
                "File name pattern that the matcher use to discover files. Use '*' to represent wildcards in file and directory names. Use '**' to represent arbitrary directory depth. Use '..' to represent a parent directory.");

            _baseUrl = cmd.Argument(
                "<BASEURL>",
                "Base URL to the dataentry portal. Should be a valid URL.");

            _dryRun = cmd.Option(
                "-n |--dryrun",
                "Do nothing; only show what would happen.",
                CommandOptionType.NoValue);

            _truncate = cmd.Option(
                "-t | --truncate",
                "Delete all sequences on the server before upload.",
                CommandOptionType.NoValue);

            _output = cmd.Option(
                "-o | --output <FILEPATH>",
                "Path to report file.",
                CommandOptionType.SingleValue);

            _quiet = cmd.Option(
                "--quiet",
                "Suppresses all output except warnings and errors.",
                CommandOptionType.NoValue);

            _clientId = cmd.Option(
                "--client-id",
                "",
                CommandOptionType.SingleValue);

            _clientSecret = cmd.Option(
                "--client-secret",
                "",
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

            if (string.IsNullOrEmpty(_output.Value()))
            {
                throw new ArgumentNullException(_output.LongName);
            }

            if (string.IsNullOrEmpty(Path.GetFileName(_output.Value())))
            {
                throw new ArgumentException(_output.LongName);
            }
        }

        protected override int Execute()
        {
            var reporter = new Reporter(_quiet.HasValue());

            var pathRoot = Path.GetPathRoot(_filePath.Value);
            var isPathRooted = string.IsNullOrWhiteSpace(pathRoot) == false;

            var pattern = _filePath.Value;
            pattern = isPathRooted
                ? pattern.Substring(pathRoot.Length)
                : pattern;
            var directory = isPathRooted
                ? pathRoot
                : Directory.GetCurrentDirectory();

            reporter.WriteOutput($"Looking for all files matching pattern - \"{pattern}\"");
            reporter.WriteOutput($"Directory specified for all files matching pattern - \"{directory}\"");

            var matchedFiles = new Matcher()
                .AddInclude(pattern)
                .GetResultsInFullPath(directory)
                .ToList();

            reporter.WriteOutput($"There are {matchedFiles.Count} files mathing");

            if (matchedFiles.Any() == false)
            {
                throw new InvalidOperationException("There are no files mathing specified pattern");
            }

            reporter.WriteOutput("Try parse all matching files");
            var payload = matchedFiles
                .SelectMany(fn => ParseSequencePayload(File.OpenRead(fn)))
                .ToList();

            reporter.WriteOutput("Building API request from specified options and arguments");
            var requestUrl = _baseUrl.Value
                .AppendPathSegments(Constants.ApiSegments.Api, Constants.ApiSegments.Sequence)
                .SetQueryParams(new
                {
                    dryRun = _dryRun.HasValue(),
                    truncate = _truncate.HasValue(),
                });

            reporter.WriteOutput($"Sending request to the server \"{requestUrl}\"");
            var response = requestUrl
                .PostJsonAsync(payload) //throw exception if (IsSuccessStatusCode == false)
                .ReceiveJson<IEnumerable<SequenceResponsePayload>>()
                .Result
                .ToList();

            
            var outputDirectory = Path.GetDirectoryName(_output.Value());

            if (string.IsNullOrEmpty(outputDirectory) == false && Directory.Exists(outputDirectory) == false)
            {
                Directory.CreateDirectory(outputDirectory);
            }

            if (File.Exists(_output.Value()))
            {
                File.Delete(_output.Value());
            }
            var fileOutputWriter = new StreamWriter(File.OpenWrite(_output.Value()));

            reporter.WriteOutput($"Save server report to the file {_output.Value()}");
            WriteOutput(fileOutputWriter, response);

            return base.Execute();
        }

        private void WriteOutput(TextWriter outputWriter, IEnumerable<SequenceResponsePayload> data)
        {
            using (var csvWriter = new CsvWriter(outputWriter))
            {
                csvWriter.Configuration.RegisterClassMap<ReportMap>();
                csvWriter.WriteHeader<SequenceResponsePayload>();
                foreach (var record in data)
                {
                    csvWriter.WriteRecord(record);
                }
            }
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

        private class SequenceResponsePayload : SequenceRequestPayload
        {
            public string Status { get; set; }
            public string Message { get; set; }
        }

        private class ReportMap : CsvClassMap<SequenceResponsePayload>
        {
            public ReportMap()
            {
                Map(m => m.OrganizationIdentifier).Name("organization_identifier").NameIndex(0);
                Map(m => m.PatientIdentifier).Name("patient_local_identifier").NameIndex(1);
                Map(m => m.SpecimenIdentifier).Name("specimen_local_identifier").NameIndex(2);
                Map(m => m.SpecimenCollectedDate).Name("specimen_collected_date").NameIndex(3);
                Map(m => m.SourceOrganism).Name("ncbi_source_organism").NameIndex(4);
                Map(m => m.TaxonIdentifier).Name("ncbi_taxon_id").NameIndex(5);
                Map(m => m.BioProject).Name("ncbi_bio_project_accession").NameIndex(6);
                Map(m => m.BioSample).Name("ncbi_bio_sample_accession").NameIndex(7);
                Map(m => m.SraIdentifiers).Name("ncbi_sra_accession").NameIndex(8);
                Map(m => m.Status).Name("status").NameIndex(9);
                Map(m => m.Message).Name("message").NameIndex(10);
            }
        }
    }
}