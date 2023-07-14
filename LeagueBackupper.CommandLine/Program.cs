﻿using System.Diagnostics.CodeAnalysis;
using CommandLine;
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

return Parser.Default.ParseArguments<BackupOptions, ExtractOptions>(args)
    .MapResult(
        (BackupOptions opts) => Backup(opts),
        (ExtractOptions opts) => Extract(opts),
        errs => 1);

[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(BackupOptions))]
[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ExtractOptions))]
int Backup(BackupOptions options)
{
    try
    {
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
        options.PatchBackupStorageFolder,
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