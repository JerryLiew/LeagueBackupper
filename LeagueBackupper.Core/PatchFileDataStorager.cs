using LeagueBackupper.Core.Structure;

namespace LeagueBackupper.Core;

public abstract class PatchFileDataStorager
{
    public abstract void Init(PatchInfo patchInfo);
    public abstract void WritePatchFile(PatchFileInfo pf, Stream clientFileStream);
    public abstract void Complete();
}