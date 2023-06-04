using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using LeagueBackupper.Core.Pipeline;
using LeagueBackupper.Core.Structure;

namespace LeagueBackupper.Core;

public class PatchExtractPipeline : IPatchExtractPipeline
{
    private readonly PatchManager _patchManager;
    private readonly PatchDataProvider _patchDataProvider;
    private readonly ExtractedPatchDataProcessor _extractedPatchDataProcessor;
    private Stopwatch _timeCounter = new Stopwatch();

    public PatchExtractPipeline(PatchDataProvider patchDataProvider, PatchManager patchManager,
        ExtractedPatchDataProcessor extractedPatchDataProcessor)
    {
        _patchDataProvider = patchDataProvider;
        _patchManager = patchManager;
        _extractedPatchDataProcessor = extractedPatchDataProcessor;
    }

    public void Extract(string clientVersion)
    {
        Log.Info($"Start extract. version:{clientVersion}");
        _timeCounter.Start();
        var containsVersion = _patchManager.ContainsPatch(clientVersion);
        if (!containsVersion)
        {
            throw new PatchNotFoundException(
                $"The clientVersion:{clientVersion} does not exists in version manager.");
        }

        PatchInfo patchInfo = _patchManager.GetPatchInfo(clientVersion);
        _extractedPatchDataProcessor.Init(patchInfo);
        _patchDataProvider.Init(patchInfo);

        var versionFiles = patchInfo.PatchFiles;
        foreach (var vf in versionFiles)
        {
            Log.Info($"extracting:{vf.Filename} length:{vf.Length}");
            using Stream stream = _patchDataProvider.ResolvePatchFileStream(vf);
            _extractedPatchDataProcessor.ProcessPatchFileStream(vf, stream);
        }

        _extractedPatchDataProcessor.Complete();
        TimeSpan timeCounterElapsed = _timeCounter.Elapsed;
        Log.Info($"Extract Finished.TimeCost:{_timeCounter.Elapsed:mm\\mss\\s}");
    }
}