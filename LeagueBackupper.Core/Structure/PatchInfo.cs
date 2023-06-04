using System.Diagnostics.CodeAnalysis;

namespace LeagueBackupper.Core.Structure;

public class PatchInfo
{
    public required string PatchVersion { get; set; }

    [SetsRequiredMembers]
    public PatchInfo(string patchVersion)
    {
        PatchVersion = patchVersion;
    }

    public PatchInfo()
    {
        
    }
    public List<PatchFileInfo> PatchFiles { get; set; }= new();
}