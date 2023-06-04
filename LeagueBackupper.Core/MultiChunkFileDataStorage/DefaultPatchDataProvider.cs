using LeagueBackupper.Core.MultiChunkFile;
using LeagueBackupper.Core.MultiChunkFileDataStorage.DataBlock;
using LeagueBackupper.Core.Structure;

namespace LeagueBackupper.Core.MultiChunkFileDataStorage;

public class DefaultPatchDataProvider : PatchDataProvider
{
    private readonly PatchMultiChunkFileInfoManager _patchMultiChunkFileInfoManager = null!;
    private readonly ChunkDataProvider _chunkDataProvider;
    private Dictionary<string, MultiChunkFileInfo> _multiChunkFileInfos = null!;

    public DefaultPatchDataProvider(PatchMultiChunkFileInfoManager patchMultiChunkFileInfoManager,
        ChunkDataProvider chunkDataProvider)
    {
        _patchMultiChunkFileInfoManager = patchMultiChunkFileInfoManager;
        _chunkDataProvider = chunkDataProvider;
    }

    public override void Init(PatchInfo patchInfo)
    {
        var temp = _patchMultiChunkFileInfoManager.Get(patchInfo.PatchVersion);
        _multiChunkFileInfos = temp.ToDictionary(info => info.FileHash, info => info);
        _chunkDataProvider.Init(patchInfo.PatchVersion);
    }

    public override Stream ResolvePatchFileStream(PatchFileInfo pf)
    {
        var multiChunkFileInfo = _multiChunkFileInfos[pf.Hash];
        var chunkInfos = multiChunkFileInfo.Chunks;
        List<Stream> subSteams = chunkInfos.Select(info => _chunkDataProvider.OpenRead(info.Hash)).ToList();
        AggregatedStream mbs = new AggregatedStream(subSteams);
        return mbs;
    }
}