namespace LeagueBackupper.Core.MultiChunkFile;

public class MultiChunkFileInfo
{
    public string FileHash { get; set; }
    public long Length { get; set; }
    public List<ChunkInfo> Chunks { get; set; }
}