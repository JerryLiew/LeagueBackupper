using System.Text.Json;

namespace LeagueBackupper.Core.MultiChunkFile;

public class JsonPatchMultiChunkFileInfoManager : PatchMultiChunkFileInfoManager
{
    private Dictionary<string, MultiChunkFileInfo> _patchFiles = new();
    private readonly string _rootFolder;

    public JsonPatchMultiChunkFileInfoManager(string rootFolder)
    {
        if (!Directory.Exists(rootFolder))
        {
            Directory.CreateDirectory(rootFolder);
        }
        _rootFolder = rootFolder;
    }

    public override List<MultiChunkFileInfo> Get(string patchVersion)
    {
        var jsonFilePath = Path.Combine(_rootFolder, $"{patchVersion}.json");
        var jsonText = File.ReadAllText(jsonFilePath);
        var fileInfos = JsonSerializer.Deserialize<List<MultiChunkFileInfo>>(jsonText);
        return fileInfos!;
    }

    public override void Save(string patchVersion, List<MultiChunkFileInfo> multiChunkFileInfos)
    {
        var jsonFilePath = Path.Combine(_rootFolder, $"{patchVersion}.json");
        var jsonText = JsonSerializer.Serialize(multiChunkFileInfos);
        File.WriteAllText(jsonFilePath, jsonText);
    }
}