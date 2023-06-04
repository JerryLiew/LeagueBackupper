using LeagueBackupper.Core.Pipeline.MultiChunkFileDataStorage;
using LeagueBackupper.Core.Structure;

namespace LeagueBackupper.Core.MultiChunkFileDataStorage;
    
public abstract class ChunkExistChecker
{
    public abstract void Init(PatchInfo patchInfo);
    public abstract bool Check(string hash);
    public abstract void Add(ChunkInfoCacheData info);
}