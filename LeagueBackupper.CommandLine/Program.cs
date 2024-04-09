using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.Json;
using CommandLine;
using LeagueBackupper.CommandLine;
using LeagueBackupper.CommandLine.Command;
using LeagueBackupper.Core;
using LeagueBackupper.Core.Extract;
using LeagueBackupper.Core.Pipeline;
using Serilog;

LoggerConfiguration logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Console()
    ;
// Log.Logger = new LoggerConfiguration()
//     .MinimumLevel.Information()
//     .Enrich.FromLogContext()
//     .WriteTo.Console()
//     // .WriteTo.File("logs/info.log", LogEventLevel.Information)
//     // .WriteTo.File("logs/err.log", LogEventLevel.Error)
//     .CreateLogger();
// Test.MyStreamTest();
// Test.Export();\

foreach (var s in args)
{
    Console.WriteLine(s);
}

// Shared.RepoInfo = new RepoInfo();
// Shared.RepoInfo.Version = "1.0.0.0";
// Shared.RepoInfo.PatcheVersions = new List<string>();
// Shared.RepoInfo.PatcheVersions.Add("12.23.45.456");
// Shared.RepoInfo.PatcheVersions.Add("12.23.15.456");
// Shared.RepoInfo.PatcheVersions.Add("12.23.5t5.456");
// Shared.RepoInfo.PatcheVersions.Add("12.23.467.456");
// Shared.SaveRepoInfo();

return Parser.Default.ParseArguments<BackupOptions, ExtractOptions, UpdateCheckOption>(args)
    .MapResult(
        (BackupOptions opts) => Backup(opts),
        (ExtractOptions opts) => Extract(opts),
        (UpdateCheckOption opts) => UpdateCheck(opts),
        (RepoUpdateOption opts) => UpdateRepoVersion(opts),
        errs => 1);

[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(BackupOptions))]
[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ExtractOptions))]
[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(RepoInfo))]
int Backup(BackupOptions options)
{
    try
    {
        var infoFile = Path.Join(options.RepoFolder, "info.json");
        if (!File.Exists(infoFile))
        {
            RepoInfo repoInfo = new RepoInfo();
            repoInfo.Version = RepoUpdater.GetCurVersion().ToString();
            string serialize = JsonSerializer.Serialize(repoInfo);
            File.WriteAllText(infoFile, serialize);
        }

        Shared.LoadRepoInfo(options.RepoFolder);
        PatchBackupPipelineBuilder
            builder = new DefaultPatchBackupPipelineBuilder(options.GameFolder, options.RepoFolder);
        var backupPipeline = builder.Build();
        backupPipeline.ClientVersionFetched += s =>
        {
            Log.Logger =
                logger.WriteTo.File($"logs/backup_{s}.log").CreateLogger();
            LeagueBackupper.Core.Core.SetLogger(Log.Information, Log.Warning, Log.Error);
        };
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

int Extract(ExtractOptions options)
{
    PatchExtractPipelineBuilder builder = new DefaultPatchExtractPipelineBuilder(
        options.RepoFolder,
        options.OutputFolder,
        options.ValidateOnly
    );
    PatchExtractPipeline pipeline = builder.Build();
    try
    {
        var logPrefix = options.ValidateOnly ? "validate" : "extract";
        Log.Logger =
            logger.WriteTo.File($"logs/{logPrefix}_{options.PatchVersion}.log")
                .CreateLogger();
        LeagueBackupper.Core.Core.SetLogger(Log.Information, Log.Warning, Log.Error);
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

static int UpdateCheck(UpdateCheckOption opt)
{
    string outputMsg = string.Empty;
    if (!Path.Exists(opt.RepoFolder))
    {
        throw new DirectoryNotFoundException(opt.RepoFolder);
    }

    RepoUpdater updater = new RepoUpdater(opt.RepoFolder);
    Version repoVersion = updater.GetRepoVersion();
    Version latestVersion = RepoUpdater.GetCurVersion();
    outputMsg = $"repo ver:{repoVersion} latest ver:{latestVersion}";
    Console.WriteLine(outputMsg);
    return 0;
}

static int UpdateRepoVersion(RepoUpdateOption opts)
{
    RepoUpdater updater = new RepoUpdater(opts.RepoFolder);
    Version dst = new Version(opts.DstVersion);
    Version curVer = updater.GetRepoVersion();
    bool updateResult = updater.Update(RepoUpdater.GetCurVersion());
    if (updateResult)
    {
        Console.WriteLine("Repository update success.");
        return 0;
    }

    Console.WriteLine("Repository update failed.");
    return 1;
}