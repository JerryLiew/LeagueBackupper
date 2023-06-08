namespace LeagueBackupper.Core.Pipeline.MultiChunkFileDataStorage;

public class ChunkInfoCacheData
{
    public required string ChunkHash { get; set; }
    public required string Url { get; set; }
}