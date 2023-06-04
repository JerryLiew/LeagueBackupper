namespace LeagueBackupper.Core.Pipeline;

public interface IPatchExtractPipeline
{
    public void Extract(string patchVersion);
}