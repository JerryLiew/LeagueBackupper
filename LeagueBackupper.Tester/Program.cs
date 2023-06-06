using CommandLine;
using LeagueBackupper.Tester;
using LeagueBackupper.Tester.Commands;
using Serilog;
using Serilog.Events;

internal class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("info.log", LogEventLevel.Information)
            .WriteTo.File("err.log", LogEventLevel.Error)
            .CreateLogger();
        Parser.Default.ParseArguments<ValidateOptions>(args)
            .MapResult(
                Validate,
                errs => 1);
    }

    static int Validate(ValidateOptions options)
    {
        CommandLineTester tester = new CommandLineTester();
        tester.Run(options);
        return 0;
    }
}