namespace LeagueBackupper.Core.DataBlock;

public abstract class ChunkDataStorager
{
    public abstract void Init(string patchVersion);
    public abstract string Write(string hash, Stream stream);
    public abstract void Flush();
}