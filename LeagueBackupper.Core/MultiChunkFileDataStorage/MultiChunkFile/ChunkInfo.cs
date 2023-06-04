namespace LeagueBackupper.Core.MultiChunkFile;

public class ChunkInfo
{
    public string Hash { get; set; }
    public long Offset { get; set; }
    public long Length { get; set; }
}