using LeagueBackupper.Core.Structure;

namespace LeagueBackupper.Core;

public abstract class PatchDataProvider
{
    public abstract void Init(PatchInfo patchInfo);
    public abstract Stream ResolvePatchFileStream(PatchFileInfo pf);
}