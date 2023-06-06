using CommandLine;

namespace LeagueBackupper.Tester.Commands;

[Verb("validate", HelpText = "back up a league client patch.")]
public class ValidateOptions
{
    [Option('c', "config", Required = true, HelpText = "Config file.")]
    public required string Cfg { get; set; }

    [Option("backup-only", Required = false, HelpText = "Specify whether backup only.")]
    public bool BackupOnly { get; set; }
}