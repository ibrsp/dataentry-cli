using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;

namespace DataEntry.Cli
{
    internal class Reporter
    {
        private readonly bool _queit;
        private readonly AnsiConsole _error = AnsiConsole.GetError(true);
        private readonly AnsiConsole _out = AnsiConsole.GetOutput(true);

        private const string Reset = "\x1b[22m\x1b[39m";
        private const string Bold = "\x1b[1m";
        private const string Red = "\x1b[31m";
        private const string Yellow = "\x1b[33m";


        public Reporter(bool queit)
        {
            _queit = queit;
        }

        private string Colorize(string value, Func<string, string> colorizeFunc)
        {
            return colorizeFunc(value);
        }

        public void WriteError(string message)
        {
            _error.WriteLine(Prefix("error:   ", Colorize(message, x => Bold + Red + x + Reset)));
        }


        public void WriteWarning(string message)
        {
            _out.WriteLine(Prefix("warn:    ", Colorize(message, x => Bold + Yellow + x + Reset)));
        }


        public void WriteOutput(string message)
        {
            if (_queit)
            {
                return;
            }
            _out.WriteLine(Prefix("info:    ", message));
        }


        private string Prefix(string prefix, string value)
        {
            return string.Join(
                Environment.NewLine,
                value.Split(new[] {Environment.NewLine}, StringSplitOptions.None).Select(l => prefix + l));
        }
    }
}