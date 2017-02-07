namespace DataEntry.Cli
{
    internal static class Constants
    {
        internal static class CommandOptions
        {
            internal const string HelpOption = "-? | -h | --help";
        }

        internal static class ExitCodes
        {
            internal const int Bad = 0xbad;
            internal const int Ok = 0;
        }

        internal static class ApiSegments
        {
            public const string Api = "api";
            public const string Sequence = "sequence";
        }
    }
}