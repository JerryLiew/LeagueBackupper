using LeagueBackupper.Core.Structure;

namespace LeagueBackupper.Core;

public abstract class PatchManager
{
    public abstract PatchInfo GetPatchInfo(string patchVersion);

    public abstract void RecordPatch(PatchInfo version);

    public abstract bool ContainsPatch(string patchVersion);

    public abstract List<string> GetAllPatchesVersion();
}