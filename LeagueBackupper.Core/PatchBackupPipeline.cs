using System.Diagnostics;
using LeagueBackupper.Common.Utils;
using LeagueBackupper.Core.Pipeline;
using LeagueBackupper.Core.Structure;
using LeagueToolkit.Core.Environment;

namespace LeagueBackupper.Core;

public class PatchBackupPipeline
{
    public event Action<string> ClientVersionFetched;
    private readonly ClientDataProvider _clientDataProvider;
    private readonly PatchManager _patchManager;
    private readonly PatchFileDataStorager _patchFileDataStorager;
    private Stopwatch _timeCounter = new Stopwatch();

    public PatchBackupPipeline(ClientDataProvider clientDataProvider, PatchManager patchManager,
        PatchFileDataStorager patchFileDataStorager)
    {
        _clientDataProvider = clientDataProvider;
        _patchManager = patchManager;
        _patchFileDataStorager = patchFileDataStorager;
    }

    public void Backup()
    {
        //get client information.
        Log.Info("Start backup.");
        _timeCounter.Start();
        var clientVersion = _clientDataProvider.GetClientVersion();
        ClientVersionFetched(clientVersion);
        Log.Info($"Client Version:{clientVersion}");
        bool exists = _patchManager.ContainsPatch(clientVersion);
        if (exists)
        {
            throw new PatchExistException(
                $"The ClientVersion: {clientVersion} already exists in version manager");
        }

        //resolve the patchInfo
        PatchInfo patchInfo = new PatchInfo(clientVersion);
        var clientFileInfos = _clientDataProvider.EnumerateFiles();
        foreach (var info in clientFileInfos)
        {
            Log.Info($"Compute hash:{info.Filename}");
            byte[] clientFileHash = _clientDataProvider.GetClientFileHash(info.Filename);
            var hashStr = Utils.GetHashString(clientFileHash);
            patchInfo.PatchFiles.Add(new PatchFileInfo(info.Filename, info.Length, hashStr, clientVersion));
        }

        _patchFileDataStorager.Init(patchInfo);
        foreach (var vf in patchInfo.PatchFiles)
        {
            Log.Info($"Processing file:{vf.Filename} len:{vf.Length} hash:{vf.Hash}");
            using Stream fileStream = _clientDataProvider.GetClientFileStream(vf.Filename);
            _patchFileDataStorager.WritePatchFile(vf, fileStream);
        }

        _patchFileDataStorager.Complete();
        _patchManager.RecordPatch(patchInfo);
        Shared.RepoInfo.PatchVersions.Add(clientVersion);
        Shared.SaveRepoInfo();
        Log.Info($"Backup Finished.TimeCost:{_timeCounter.Elapsed:mm\\mss\\s}");
    }
}