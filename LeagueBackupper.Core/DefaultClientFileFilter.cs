using System.Text.RegularExpressions;
using LeagueBackupper.Core.Pipeline;

namespace LeagueBackupper.Core;

internal class DefaultClientFileFilter : IClientFileFilter
{
    private static readonly Regex FilterRegex = new Regex("^Logs.*");

    public bool ShouldExclude(string path)
    {
        if (FilterRegex.IsMatch(path))
        {
            return true;
        }
        // if (path.Contains("GameLogs"))
        // {
        //     return true;
        // }

        return false;
    }
}