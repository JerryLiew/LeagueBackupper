using LeagueBackupper.Core.Structure;

namespace LeagueBackupper.Core.MultiChunkFile;

public abstract class PatchMultiChunkFileInfoManager
{
    public abstract List<MultiChunkFileInfo> Get(string patchVersion);

    public abstract void Save(string patchVersion,List<MultiChunkFileInfo> multiChunkFileInfos);
}