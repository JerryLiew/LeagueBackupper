using CommandLine;
using LeagueBackupper.CommandLine.Command;
using LeagueBackupper.Core;
using LeagueBackupper.Core.Pipeline;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("info.log",LogEventLevel.Information)
    .WriteTo.File("err.log",LogEventLevel.Error)
    .CreateLogger();
LeagueBackupper.Core.Core.SetLogger(s => Log.Information(s), s => Log.Warning(s), s => Log.Error(s));
// Test.MyStreamTest();
// Test.Export();
Parser.Default.ParseArguments<BackupOptions, ExtractOptions>(args)
    .MapResult(
        (BackupOptions opts) => Backup(opts),
        (ExtractOptions opts) => Extract(opts),
        errs => 1);

static int Backup(BackupOptions options)
{
    PatchBackupPipelineBuilder
        builder = new DefaultPatchBackupPipelineBuilder(options.GameFolder, options.BackupFolder);
    var backupPipeline = builder.Build();
    backupPipeline.Backup();

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
    pipeline.Extract(options.PatchVersion);
    return 0;
}