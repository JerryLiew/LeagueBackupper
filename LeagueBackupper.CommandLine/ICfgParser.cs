namespace LeagueBackupper.CommandLine;

public interface ICfgParser<T>
{
    public void CfgParser(ref T p);
}