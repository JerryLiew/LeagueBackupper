using CommandLine;

namespace LeagueBackupper.Tester.Commands;

[Verb("validate", HelpText = "back up a league client patch.")]
public class ValidateOptions
{
    [Option('c', "config", Required = true, HelpText = "Config file.")]
    public required string Cfg { get; set; }

    [Option('r', "process-randomly", Required = false, Default = false, HelpText = "Specify whether process clients randomly.")]

    public bool ProcessRandomly { get; set; }
    
    [Option ("validate-one",Required = false, Default = false, HelpText = "Specify whether validate subsequently.")]

    public bool ValidateAfterBackupOne { get; set; }
    
    [Option( "validate-all",Required = false, Default = false, HelpText = "Specify whether validate after all clients was backup completed.")]

    public bool ValidateAfterBackupAll { get; set; }

}