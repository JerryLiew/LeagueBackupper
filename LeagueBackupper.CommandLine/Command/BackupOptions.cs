using CommandLine;

namespace LeagueBackupper.CommandLine.Command;

public abstract class BaseOptions
{
    // [Option('c', Required = false, HelpText = "Specify a cfg file.")]
    // public string? ConfigFile { get; set; }
}

[Verb("backup", HelpText = "back up a league client patch.")]
public partial class BackupOptions : BaseOptions
{
    [Option('g', "game-folder", Required = false,
        HelpText = "The game folder which contains game client you want to backup.")]
    public required string GameFolder { get; set; }

    // [Option("patch-folder", Required = false, HelpText = "The folder for storing patch.")]
    // public required string PatchFolder { get; set; }

    [Option('z', "zip-file", Required = false,
        HelpText = "The zip file which contains game client you want to backup.")]
    public string? ZipFile { get; set; }

    [Option("temp-folder", Required = false, HelpText = "The zip file which contains game client you want to backup.")]

    public string? TempFolder { get; set; }

    [Option('r', "repo-folder", Required = true, HelpText = "The folder for storing all backup data.")]
    public required string RepoFolder { get; set; }
}

[Verb("extract", HelpText = "extract a league client patch.")]
public class ExtractOptions : BaseOptions
{
    [Option('v', "patch-version", Required = true, HelpText = "Give a patch version to extract.")]
    public required string PatchVersion { get; set; }

    // [Option("patch-folder", Required = false, HelpText = "The folder for storage patch.")]
    // public required string PatchFolder { get; set; }

    [Option('r', "repo-folder for storing all backup data.")]
    public required string RepoFolder { get; set; }

    [Option('o', "output-folder", Required = false, HelpText = "The folder for extract to.")]
    public required string OutputFolder { get; set; }

    // [Option( "multidb-file", Required = false, HelpText = "The multi-data block files description file. ")]
    // public required string MultiDbFile { get; set; }

    [Option("validate-only", HelpText = "Indicate that current extraction operation is for validating patch.")]
    public bool ValidateOnly { get; set; } = false;

    // [Option('z', longName: "zip-file", Required = false,
    //     HelpText = "Specify the zip file/folder to provide data block read stream.")]
    // public required IEnumerable<string> DataBlockZipFiles { get; set; }
}

public class UpdateCheckOption
{
    [Option('r', Required = true, HelpText = "repo-folder for storing all backup data.")]
    public required string RepoFolder { get; set; }
}

public class RepoUpdateOption
{
    [Option('r', Required = true, HelpText = "repo-folder for storing all backup data.")]
    public required string RepoFolder { get; set; }

    [Option('v', HelpText = "The destination version which you want to update to.")]
    public required string DstVersion { get; set; }
}