using System.Collections;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CliWrap;
using LeagueBackupper.Tester.Commands;
using Serilog;
using YamlDotNet.Serialization;

namespace LeagueBackupper.Tester;

public class CommandLineTesterCfg
{
    public List<string> ClientFolders { get; set; }

    public string CommandLineExePath { get; set; }

    public string RepositoyPath { get; set; }

    public bool ProcessRandomly { get; set; } = false;

    public bool ValidateOne { get; set; } = true;

    public bool ValidateAll { get; set; } = true;
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

    public void CreateCfg()
    {
        CommandLineTesterCfg cfg = new CommandLineTesterCfg();
        cfg.CommandLineExePath = $"D:/{nameof(cfg.CommandLineExePath)}";
        cfg.ClientFolders = new() { "C:/folder1", "C:/folder2", "C:/folder3" };
        cfg.RepositoyPath = "D:/repositoryPath";
        SerializerBuilder builder = new SerializerBuilder();
        ISerializer serializer = builder.Build();
        var serialize = serializer.Serialize(cfg);
        File.WriteAllText("D:/testYaml.yaml", serialize);
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


    public void Run(ValidateOptions cfg)
    {
        string readAllText = File.ReadAllText(cfg.Cfg);
        DeserializerBuilder builder = new DeserializerBuilder();
        var deserializer = builder.Build();
        Cfg = deserializer.Deserialize<CommandLineTesterCfg>(readAllText);
        List<string> clientsFolder = new List<string>(Cfg.ClientFolders);
        if (Cfg.ProcessRandomly)
        {
            clientsFolder = ShuffleList(clientsFolder);
        }

        List<string> versions = new List<string>();
        foreach (var cf in clientsFolder)
        {
            Log.Information("Start processing: {ClientFolder}", cf);
            string exeFilename = Path.Combine(cf, "League of Legends.exe");
            FileVersionInfo version = FileVersionInfo.GetVersionInfo(exeFilename);
            versions.Add(version.ProductVersion!);
            int backup = Backup(Cfg.CommandLineExePath, cf, Cfg.RepositoyPath);
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
                int validateResult = Validate(Cfg.CommandLineExePath, Cfg.RepositoyPath, version.ProductVersion);
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
                int validateResult = Validate(Cfg.CommandLineExePath, Cfg.RepositoyPath, ver);
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
        var result = Cli
            .Wrap(
                command)
            .WithArguments(
                new[]
                {
                    "backup",
                    "-g", $"{clientFolder}",
                    "-b", $"{repo}"
                })
            .WithStandardOutputPipe(PipeTarget.ToDelegate(s => { Log.Debug(s); }))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(s => Log.Error(s)))
            .WithValidation(CommandResultValidation.None)
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(builder))
            .ExecuteAsync().GetAwaiter().GetResult();
        // Console.WriteLine(builder.ToString());

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
                    "-b", $"{repo}",
                    "--validate-only",
                })
            .WithStandardOutputPipe(PipeTarget.ToDelegate(s => { Log.Debug(s); }))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(s => Log.Error(s)))
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(builder))
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync()
            .GetAwaiter().GetResult();
        // Console.WriteLine(builder.ToString());

        return result.ExitCode;
    }

    public Task Validate()
    {
        return Task.CompletedTask;
    }
}