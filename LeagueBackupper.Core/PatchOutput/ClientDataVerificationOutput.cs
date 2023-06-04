using System.Security.Cryptography;
using LeagueBackupper.Common.Utils;
using LeagueBackupper.Core.Pipeline;
using LeagueBackupper.Core.Structure;

namespace LeagueBackupper.Core.PatchOutput;

public class ClientDataValidator : ExtractedPatchDataProcessor
{
    private MD5 _md5 = null!;
    private List<PatchFileInfo> _verifyFailedResult = null!;
    public bool Identical => _verifyFailedResult.Count == 0;

    public override void Init(PatchInfo version)
    {
        _verifyFailedResult = new();
        _md5 = MD5.Create();
    }

    public override void ProcessPatchFileStream(PatchFileInfo patchFileInfo, Stream outputStream)
    {
        Log.Info($"Verifying file: {patchFileInfo.Filename}");
        byte[] computeHash = _md5.ComputeHash(outputStream);
        string hashString = Utils.GetHashString(computeHash);
        if (patchFileInfo.Hash != hashString)
        {
            _verifyFailedResult.Add(patchFileInfo);
        }
    }

    public override void Complete()
    {
        Log.Info($"File validate completed! Validate failed count:{_verifyFailedResult.Count}");
        foreach (var vf in _verifyFailedResult)
        {
            Log.Err($"Corrupted file:{vf.Filename}");
        }
    }
}