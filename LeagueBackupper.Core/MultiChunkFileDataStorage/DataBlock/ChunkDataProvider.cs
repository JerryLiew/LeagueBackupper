using LeagueBackupper.Core.DataBlock;

namespace LeagueBackupper.Core.MultiChunkFileDataStorage.DataBlock;

public abstract class ChunkDataProvider : IDisposable
{
    public abstract Stream OpenRead(string hash);

    public virtual void Init(string patchVersion)
    {
    }

    public abstract bool TryGetDataBlockInfo(string hash, out DataBlockInfo? dataBlockInfo);

    public abstract List<DataBlockInfo> GetAllDataBlockInfo();

    public virtual void Dispose()
    {
    }
}