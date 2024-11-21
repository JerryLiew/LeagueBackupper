using System.Reflection;
using System.Text.Json.Serialization;
using LeagueBackupper.Core;

namespace LeagueBackupper.CommandLine;

public class RepoUpdater
{
    private string _repoPath;
    private string RepoInfoFilename => Path.Join(_repoPath, "info.json");

    public RepoUpdater(string repoPath)
    {
        _repoPath = repoPath;
    }

    public void Init(string repoPath)
    {
        _repoPath = repoPath;
    }

    public static Version GetCurVersion()
    {
        return Assembly.GetEntryAssembly()!.GetName().Version!;
    }

    public Version GetRepoVersion()
    {
        bool exists = File.Exists(RepoInfoFilename);
        if (!exists)
        {
            return new Version("1.0.0.0");
        }

        Shared.LoadRepoInfo(_repoPath);
        return new Version(Shared.RepoInfo.Version);
    }

    public Version GetMinimumSupportedVersion()
    {
        return new Version("1.1.1.0");
    }


    public bool Update(Version dstVer)
    {
        return true;
    }
}