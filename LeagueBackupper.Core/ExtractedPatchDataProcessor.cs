using LeagueBackupper.Core.Structure;

namespace LeagueBackupper.Core.Pipeline;

public abstract class ExtractedPatchDataProcessor
{
    public abstract void Init(PatchInfo patchInfo);
    public abstract void ProcessPatchFileStream(PatchFileInfo patchFileInfo, Stream outputStream);
    public abstract void Complete();
}