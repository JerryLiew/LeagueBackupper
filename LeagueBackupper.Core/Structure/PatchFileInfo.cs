namespace LeagueBackupper.Core.Structure;

public record PatchFileInfo(string Filename, long Length, string Hash, string PatchVersion);