using System.Text.Json;

namespace LeagueBackupper.Core;

public static class Shared
{
    public static RepoInfo RepoInfo;
    private static string _repoFolder;

    public static string RepoInfoFileName => Path.Join(_repoFolder, "info.json");

    public static void LoadRepoInfo(string repoPath)
    {
        if (!File.Exists(RepoInfoFileName))
        {
            throw new FileNotFoundException("InfoFilename: The given file doest not exist.");
        }

        string readAllText = File.ReadAllText(RepoInfoFileName);
        RepoInfo? repoInfo = JsonSerializer.Deserialize<RepoInfo>(readAllText);
        RepoInfo = repoInfo!;
        if (RepoInfo.PatcheVersions == null)
        {
            RepoInfo.PatcheVersions = new List<string>();
        }
    }

    public static void SaveRepoInfo()
    {
        string serialize = JsonSerializer.Serialize(RepoInfo,new JsonSerializerOptions()
        {
            WriteIndented = true
        });
        File.WriteAllText(RepoInfoFileName, serialize);
    }
}