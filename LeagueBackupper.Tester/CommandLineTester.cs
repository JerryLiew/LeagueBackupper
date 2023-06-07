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


    public async void Run(ValidateOptions cfg)
    {
        string readAllText = File.ReadAllText(cfg.Cfg);
        DeserializerBuilder builder = new DeserializerBuilder();
        var deserializer = builder.Build();
        Cfg = deserializer.Deserialize<CommandLineTesterCfg>(readAllText);
         Backup();
    }

    public async void  Backup()
    {
        List<string> cfgClientFolders = Cfg.ClientFolders;
        foreach (var gameFolder in cfgClientFolders)
        {
            StringBuilder builder = new StringBuilder();
            Log.Information("Start backup {GameFolder}", gameFolder);
            CommandResult? commandResult = default;
       var result=   await Cli.Wrap("E:/WorkSpace/C#/LeagueBackupper/LeagueBackupper.CommandLine/bin/Debug/net8.0/LeagueBackupper.CommandLine.exe").WithArguments(
                    new[]
                    {
                        "--help"
                        // "E:/WorkSpace/C#/LeagueBackupper/LeagueBackupper.CommandLine/bin/Debug/net8.0/LeagueBackupper.CommandLine.exe",
                        // "backup",
                        // "-g", $"{gameFolder}",
                        // "-b", $"{Cfg.RepositoyPath}"
                    })
                .WithStandardOutputPipe(PipeTarget.ToDelegate(s => { Log.Information(s); }))
                .WithStandardErrorPipe(PipeTarget.ToDelegate(s => Log.Error(s)))
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(builder))
                .ExecuteAsync();
            Console.WriteLine(builder.ToString());

            if (result.ExitCode == 0)
            {
                Log.Information("Backup gameFolder: {GameFolder} finished", gameFolder);
            }
            else
            {
                Log.Error("Backup gameFolder: {GameFolder} err exit code:{ExitCode}", gameFolder,
                    result.ExitCode);
            }
        }
    }

    public Task Validate()
    {
        return Task.CompletedTask;
    }
}