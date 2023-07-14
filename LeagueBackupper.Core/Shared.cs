using System.Text.Json;

namespace LeagueBackupper.Core;

public static class Shared
{
    public static RepoInfo RepoInfo;

    public static void LoadRepoInfo(string repoPath)
    {
        var infoFilename = Path.Join(repoPath, "info.json");
        if (!File.Exists(infoFilename))
        {
            throw new FileNotFoundException("InfoFilename: The given file doest not exist.");
        }
        string readAllText = File.ReadAllText(infoFilename);
        RepoInfo? repoInfo = JsonSerializer.Deserialize<RepoInfo>(readAllText);
        RepoInfo = repoInfo;
    }
}