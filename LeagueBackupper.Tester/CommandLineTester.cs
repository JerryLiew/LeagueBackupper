using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using CliWrap;
using LeagueBackupper.Tester.Commands;
using Serilog;

namespace LeagueBackupper.Tester;

public class CommandLineTesterCfg
{
    public List<string> ClientFolders { get; set; }

    public string CommandLineExePath { get; set; }

    public string RepositoryPath { get; set; }

    public bool ProcessRandomly { get; set; } = false;

    public bool ValidateOne { get; set; } = true;

    public bool ValidateAll { get; set; } = true;

    public string? ClientsZipFolder { get; set; }
    public string? ClientUnzipTempFolder { get; set; }
}

public class CommandLineTester
{
    public static CommandLineTesterCfg Cfg;

    // public void CreateCfg()
    // {
    //     // CommandLineTesterCfg cfg = new CommandLineTesterCfg();
    //     // cfg.CommandLineExePath = $"D:/{nameof(cfg.CommandLineExePath)}";
    //     // cfg.ClientFolders = new() { "C:/folder1", "C:/folder2", "C:/folder3" };
    //     // cfg.RepositoyPath = "D:/repositoryPath";
    //     // string fromModel = Toml.FromModel(cfg);
    //     // Console.WriteLine(fromModel);
    //     // File.WriteAllText("D:/testToml.toml", fromModel);
    // }

    public void CreateCfg(string configFilePath)
    {
        CommandLineTesterCfg cfg = new CommandLineTesterCfg();
        cfg.CommandLineExePath = $"D:/{nameof(cfg.CommandLineExePath)}";
        cfg.ClientFolders = new() { "C:/folder1", "C:/folder2", "C:/folder3" };
        cfg.RepositoryPath = "D:/repositoryPath";
        string serialize = JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(configFilePath, serialize);
    }

    private static List<string> ShuffleList(List<string> src)
    {
        Random random = new Random();
        List<string> result = new List<string>(src.Count);
        src.ForEach(s => result.Add(s));
        for (var index = 0; index < result.Count; index++)
        {
            var s = result[index];
            int randomPos = random.Next(src.Count);
            string temp = result[randomPos];
            result[randomPos] = s;
            result[index] = temp;
        }

        return result;
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ValidateOptions))]
    public void Run(ValidateOptions cfg)
    {
        string readAllText = File.ReadAllText(cfg.Cfg);

        // Cfg = deserializer.Deserialize<CommandLineTesterCfg>(readAllText);
        Cfg = JsonSerializer.Deserialize<CommandLineTesterCfg>(readAllText)!;
        List<string> clientsFolder = new List<string>(Cfg.ClientFolders);
        if (Cfg.ProcessRandomly)
        {
            clientsFolder = ShuffleList(clientsFolder);
        }

        if (!string.IsNullOrEmpty(Cfg.ClientsZipFolder))
        {
            clientsFolder.Clear();
        }

        List<string> versions = new List<string>();
        foreach (var cf in clientsFolder)
        {
            Log.Information("Start processing: {ClientFolder}", cf);
            string exeFilename = Path.Combine(cf, "League of Legends.exe");
            FileVersionInfo version = FileVersionInfo.GetVersionInfo(exeFilename);
            versions.Add(version.ProductVersion!);
            int backup = Backup(Cfg.CommandLineExePath, cf, Cfg.RepositoryPath);
            if (backup == 0)
            {
                ValidateOutput.Ok($"backup:{cf}");
            }
            else
            {
                ValidateOutput.Err($"backup:{cf}");
            }

            if (Cfg.ValidateOne)
            {
                int validateResult = Validate(Cfg.CommandLineExePath, Cfg.RepositoryPath, version.ProductVersion);
                if (validateResult == 0)
                {
                    ValidateOutput.Ok($"validate-one:{version.ProductVersion}");
                    Log.Information("validate-one success! version:{Version}", version.ProductVersion);
                }
                else
                {
                    ValidateOutput.Err($"validate-one:{version.ProductVersion}");
                    Log.Error("validate-one failed! version:{Version}", version.ProductVersion);
                }
            }
        }

        if (Cfg.ValidateAll)
        {
            foreach (var ver in versions)
            {
                int validateResult = Validate(Cfg.CommandLineExePath, Cfg.RepositoryPath, ver);
                if (validateResult == 0)
                {
                    ValidateOutput.Ok($"validate-all:{ver}");
                    Log.Information("validate-all success! version:{Version}", ver);
                }
                else
                {
                    ValidateOutput.Err($"validate-all:{ver}");
                    Log.Error("validate-all failed! version:{Version}", ver);
                }
            }
        }
    }

    public int Backup(string command, string clientFolder, string repo)
    {
        StringBuilder builder = new StringBuilder();
        Log.Information("Start backup {GameFolder}", clientFolder);
        CommandResult? commandResult = default;
        var c = Cli
            .Wrap(
                command)
            .WithArguments(
                new[]
                {
                    "backup",
                    "-g", $"{clientFolder}",
                    "-r", $"{repo}"
                })
            .WithStandardOutputPipe(PipeTarget.ToDelegate(s => { Log.Debug(s); }))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(s => Log.Error(s)))
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(builder));
        var result =
            c.ExecuteAsync().GetAwaiter().GetResult();
        // Console.WriteLine(builder.ToString());
        Console.WriteLine(c.ToString());
        if (result.ExitCode == 0)
        {
            Log.Information("Backup gameFolder: {GameFolder} finished", clientFolder);
        }
        else
        {
            Log.Error("Backup gameFolder: {GameFolder} err exit code:{ExitCode}", clientFolder,
                result.ExitCode);
        }

        return result.ExitCode;
    }

    public int Validate(string command, string repo, string version)
    {
        StringBuilder builder = new StringBuilder();
        Log.Information("Start validate version:{Version}", version);
        CommandResult? commandResult = default;
        var result = Cli
            .Wrap(
                command)
            .WithArguments(
                new[]
                {
                    "extract",
                    "-v", $"{version}",
                    "-r", $"{repo}",
                    "--validate-only",
                })
            .WithStandardOutputPipe(PipeTarget.ToDelegate(s => { Log.Debug(s); }))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(s => Log.Error(s)))
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(builder))
            .ExecuteAsync().GetAwaiter().GetResult();
        // Console.WriteLine(builder.ToString());

        return result.ExitCode;
    }

    public Task Validate()
    {
        return Task.CompletedTask;
    }
}