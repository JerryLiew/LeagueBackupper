using System.Diagnostics;
using System.Security.Cryptography;
using LeagueBackupper.Common.Utils;
using LeagueBackupper.Core.Pipeline;
using LeagueBackupper.Core.Structure;

namespace LeagueBackupper.Core.ClientData;

public class GameFolderClientDataProvider : ClientDataProvider
{
    private const string LeagueExeName = "League of Legends.exe";
    private string _gameFolder;
    private IClientFileFilter? _clientFileFilter;
    private readonly MD5 _md5;

    public GameFolderClientDataProvider(string gameFolder, IClientFileFilter? clientFileFilter = null)
    {
        _gameFolder = gameFolder;
        _clientFileFilter = clientFileFilter;
        _md5 = MD5.Create();
    }


    #region Overrid Method

    public override string GetClientVersion()
    {
        var clientExeFullName = GetFileFullName(LeagueExeName);
        if (!File.Exists(clientExeFullName))
        {
            throw new FileNotFoundException($"Client file:{clientExeFullName} does not exists.");
        }

        var fileVersionInfo = FileVersionInfo.GetVersionInfo(clientExeFullName);
        var clientVersion = fileVersionInfo.ProductVersion;
        return clientVersion!;
    }

    public override List<ClientFileInfo> EnumerateFiles()
    {
        List<ClientFileInfo> result = new();
        List<Tuple<string, DirectoryInfo[], FileInfo[]>> tree = OSHelper.Walk(_gameFolder);
        foreach (var (curFolder, dis, fis) in tree)
        {
            foreach (var f in fis)
            {
                var relativePath = Path.GetRelativePath(_gameFolder, f.FullName);
                if (_clientFileFilter != null && _clientFileFilter.ShouldExclude(relativePath))
                {
                    continue;
                }

                Log.Info($"EnumerateFile: {relativePath}");
                result.Add(new ClientFileInfo() { Filename = relativePath, Length = f.Length });
            }
        }

        return result;
    }

    public PatchInfo GetPatch()
    {
        Log.Info("Get Version Start...");
        MD5 md5 = MD5.Create();
        var clientVersion = GetClientVersion();
        PatchInfo patchInfo = new PatchInfo(clientVersion);
        List<Tuple<string, DirectoryInfo[], FileInfo[]>> tree = OSHelper.Walk(_gameFolder);
        foreach (var (curFolder, dis, fis) in tree)
        {
            Log.Info($"GetVersion cd:{curFolder}");
            foreach (var f in fis)
            {
                var relativePath = Path.GetRelativePath(_gameFolder, f.FullName);
                if (_clientFileFilter != null && _clientFileFilter.ShouldExclude(relativePath))
                {
                    continue;
                }

                using var fs = f.OpenRead();
                byte[] fileHash = md5.ComputeHash(fs);
                PatchFileInfo vf = new PatchFileInfo(relativePath, f.Length, Utils.GetHashString(fileHash),
                    patchInfo.PatchVersion);
                patchInfo.PatchFiles.Add(vf);
                Log.Info($"{vf}");
            }
        }

        return patchInfo;
    }

    public override Stream GetClientFileStream(string filename)
    {
        return File.OpenRead(GetFileFullName(filename));
    }

    public override byte[] GetClientFileHash(string filename)
    {
        FileStream fileStream = File.OpenRead(GetFileFullName(filename));
        byte[] fileHash = _md5.ComputeHash(fileStream);
        return fileHash;
    }

    #endregion

    #region Private Method

    private string GetFileFullName(string relativePath)
    {
        return Path.Combine(_gameFolder, relativePath);
    }

    #endregion
}