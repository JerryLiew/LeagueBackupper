using CommandLine;

namespace LeagueBackupper.CommandLine.Command;

public abstract class BaseOptions
{
    [Option('c', Required = false, HelpText = "Specify a cfg file.")]
    public string? ConfigFile { get; set; }
}

[Verb("backup", HelpText = "back up a league client patch.")]
public partial class BackupOptions : BaseOptions
{
    [Option('g', "game-folder", Required = true,
        HelpText = "The game folder which contains game client you want to backup.")]
    public required string GameFolder { get; set; }

    [Option("patch-folder", Required = false, HelpText = "The folder for storage patch.")]
    public required string PatchFolder { get; set; }

    [Option('b',"backup-storage-folder", Required = true, HelpText = "The folder for storage all backup data.")]
    public required string BackupFolder { get; set; }
}

[Verb("extract", HelpText = "extract a league client patch.")]
public class ExtractOptions : BaseOptions
{
    [Option('v', "patch-version", Required = true, HelpText = "Give a patch version to extract.")]
    public required string PatchVersion { get; set; }

    [Option("patch-folder", Required = true, HelpText = "The folder for storage patch.")]
    public required string PatchFolder { get; set; }

    [Option("backup-storage-folder", Required = true, HelpText = "The folder for storage all backup data.")]
    public required string PatchBackupStorageFolder { get; set; }

    [Option('o', "output-folder", Required = true, HelpText = "The folder for extract to.")]
    public required string OutputFolder { get; set; }

    [Option( "multidb-file", Required = true, HelpText = "The multi-data block files description file. ")]
    public required string MultiDbFile { get; set; }
    
    [Option("validate", HelpText = "Indicate that current extraction operation is for validating patch.")]
    public bool ValidateOnly { get; set; } = false;

    [Option('z', longName: "zip-file", Required = true,
        HelpText = "Specify the zip file/folder to provide data block read stream.")]
    public required IEnumerable<string> DataBlockZipFiles { get; set; }

}