using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using LeagueBackupper.Core.Pipeline;
using LeagueBackupper.Core.Structure;

namespace LeagueBackupper.Core;

public class PatchExtractPipeline : IPatchExtractPipeline
{
    public  PatchManager PatchManager{get; private set;}
    public  PatchDataProvider PatchDataProvider{get; private set;}
    public  ExtractedPatchDataProcessor ExtractedPatchDataProcessor{get; private set;}
    private Stopwatch _timeCounter = new Stopwatch();

    public PatchExtractPipeline(PatchDataProvider patchDataProvider, PatchManager patchManager,
        ExtractedPatchDataProcessor extractedPatchDataProcessor)
    {
        PatchDataProvider = patchDataProvider;
        PatchManager = patchManager;
        ExtractedPatchDataProcessor = extractedPatchDataProcessor;
    }

    public void Extract(string clientVersion)
    {
        Log.Info($"Start extract. version:{clientVersion}");
        _timeCounter.Start();
        var containsVersion = PatchManager.ContainsPatch(clientVersion);
        if (!containsVersion)
        {
            throw new PatchNotFoundException(
                $"The clientVersion:{clientVersion} does not exists in version manager.");
        }

        PatchInfo patchInfo = PatchManager.GetPatchInfo(clientVersion);
        ExtractedPatchDataProcessor.Init(patchInfo);
        PatchDataProvider.Init(patchInfo);

        var versionFiles = patchInfo.PatchFiles;
        foreach (var vf in versionFiles)
        {
            Log.Info($"extracting:{vf.Filename} length:{vf.Length}");
            using Stream stream = PatchDataProvider.ResolvePatchFileStream(vf);
            ExtractedPatchDataProcessor.ProcessPatchFileStream(vf, stream);
        }

        ExtractedPatchDataProcessor.Complete();
        TimeSpan timeCounterElapsed = _timeCounter.Elapsed;
        Log.Info($"Extract Finished.TimeCost:{_timeCounter.Elapsed:mm\\mss\\s}");
    }
}