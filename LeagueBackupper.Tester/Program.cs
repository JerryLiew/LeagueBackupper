using System.Globalization;
using System.Text.Json;
using CommandLine;
using LeagueBackupper.Tester;
using LeagueBackupper.Tester.Commands;
using Microsoft.Win32.SafeHandles;
using Serilog;
using Serilog.Events;
using ZRUtils;
using ZRUtils.Extensions;

public static class ValidateOutput
{
    public static void Init()
    {
        if (File.Exists("result.txt"))
        {
            File.Delete("result.txt");
        }
    }

    public static void OutputResult(string msg)
    {
        File.AppendAllText("result.txt", $"{msg}\n");
    }

    public static void Ok(string msg)
    {
        OutputResult($"OK | {msg}");
    }

    public static void Err(string msg)
    {
        OutputResult($"ERR | {msg}");
    }
}

internal class Program
{
    public static async Task Main(string[] args)
    {
        ValidateOutput.Init();
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("info.log", LogEventLevel.Information)
            .WriteTo.File("err.log", LogEventLevel.Error)
            .CreateLogger();
        var parserResult =
            Parser.Default.ParseArguments<ValidateOptions, CreateCfgOptions, CollectClientsOptions>(args);
        await parserResult.MapResult(
            async (ValidateOptions o) =>
            {
                var validate = await Validate(o);
                return validate;
            }, (CreateCfgOptions o) => { return Task.FromResult(1); },
            (CollectClientsOptions o) =>
            {
                CollectClients(o);
                return Task.FromResult(1);
            },
            errs => Task.FromResult(1));
    }

    static Task<int> Validate(ValidateOptions options)
    {
        CommandLineTester tester = new CommandLineTester();
        // tester.CreateCfg();;
        tester.Run(options);
        return Task.FromResult(0);
    }

    static void CollectClients(CollectClientsOptions options)
    {
        var o = new { ClientFolders = new List<string>() { } };

        FileTreeWalker walker = new FileTreeWalker(options.Directory);
        foreach (var (root, dirs, files) in walker)
        {
            foreach (var file in files)
            {
                var substring = file.Substring(root.Length + 1);
                if (substring == "League of Legends.exe")
                {
                    o.ClientFolders.Add(root);
                }
            }
        }

        var serialize = JsonSerializer.Serialize(o,new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(serialize);
    }
}