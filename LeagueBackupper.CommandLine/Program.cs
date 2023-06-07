using CommandLine;
using LeagueBackupper.CommandLine.Command;
using LeagueBackupper.Core;
using LeagueBackupper.Core.Extract;
using LeagueBackupper.Core.PatchOutput;
using LeagueBackupper.Core.Pipeline;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("info2.log", LogEventLevel.Information)
    .WriteTo.File("err2.log", LogEventLevel.Error)
    .CreateLogger();
LeagueBackupper.Core.Core.SetLogger(s => Log.Information(s), s => Log.Warning(s), s => Log.Error(s));
// Test.MyStreamTest();
// Test.Export();
foreach (var s in args)
{
    Console.WriteLine(s);
}

Parser.Default.ParseArguments<BackupOptions, ExtractOptions>(args)
    .MapResult(
        (BackupOptions opts) => Backup(opts),
        (ExtractOptions opts) => Extract(opts),
        errs => { return 1; });

static int Backup(BackupOptions options)
{
    try
    {
        PatchBackupPipelineBuilder
            builder = new DefaultPatchBackupPipelineBuilder(options.GameFolder, options.BackupFolder);
        var backupPipeline = builder.Build();
        backupPipeline.Backup();
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
        Log.Error(e.Message);
        return 0;
    }

    /*//not use cfg file.
        ClientDataProvider clientDataProvider =
            new GameFolderClientDataProvider(options.GameFolder, new LogFilter());
        IPatchManager patchManager = new JsonPatchManager(options.PatchFolder);
        BackupDataStorage backupDataStorage = new MultiChunkFileBackupDataStorage(options.PatchBackupStorageFolder);
        string clientVersion = clientDataProvider.GetClientVersion();
        PatchBackupPipeline pipeline = new PatchBackupPipeline(clientDataProvider, patchManager, backupDataStorage);
        Log.Information(clientVersion);
        pipeline.Backup();
        PatchInfo patchInfo = clientDataProvider.GetPatch();
        patchManager.RecordPatch(patchInfo);*/

    return 0;
}

static int Extract(ExtractOptions options)
{
    PatchExtractPipelineBuilder builder = new DefaultPatchExtractPipelineBuilder(
        options.PatchBackupStorageFolder,
        options.OutputFolder,
        options.ValidateOnly
    );
    PatchExtractPipeline pipeline = builder.Build();
    try
    {
        pipeline.Extract(options.PatchVersion);
        if (options.ValidateOnly)
        {
            var validator = (ExtractedDataValidator)pipeline.ExtractedPatchDataProcessor;
            return validator.Identical ? 0 : 1;
        }

        return 0;
    }
    catch (Exception e)
    {
        Log.Error(e.ToString());
        return 1;
    }
}