using CommandLine;
using LeagueBackupper.Tester;
using LeagueBackupper.Tester.Commands;
using Serilog;
using Serilog.Events;

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
        var parserResult = Parser.Default.ParseArguments<ValidateOptions>(args);
        await parserResult.MapResult(
            async (ValidateOptions o) =>
            {
                var validate = await Validate(o);
                return validate;
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
}