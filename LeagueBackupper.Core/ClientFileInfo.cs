namespace LeagueBackupper.Core.Pipeline;

public class ClientFileInfo
{
    public required string Filename { get; init; }
    public long Length { get; init; } = -1;
}