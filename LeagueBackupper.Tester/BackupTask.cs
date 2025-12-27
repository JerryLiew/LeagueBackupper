namespace LeagueBackupper.Tester;

public enum TaskType
{
    Zipped,
    Unzipped
}
public class BackupTask
{
    public TaskType Type { get; set; }
    public string PatchVersion { get; set; }
    public string Path { get; set; }

    public override string ToString()
    {
        return $"{Type} {PatchVersion} {Path}";
    }
}