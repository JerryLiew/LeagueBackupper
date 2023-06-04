namespace LeagueBackupper.Core.Pipeline;

public interface IClientFileFilter
{
    bool ShouldExclude(string path);
}