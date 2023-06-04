using System.Text.Json;
using LeagueBackupper.Core.Pipeline.MultiChunkFileDataStorage;
using LeagueBackupper.Core.Structure;

namespace LeagueBackupper.Core.MultiChunkFileDataStorage;

public class JsonChunkExistChecker : ChunkExistChecker
{
    private Dictionary<string, ChunkInfoCacheData> _cache = new();
    private string _jsonFilePath = string.Empty;
    private readonly string _rootFolder;


    public JsonChunkExistChecker(string rootFolder)
    {
        _rootFolder = rootFolder;
    }

    public override void Init(PatchInfo patchInfo)
    {
        _jsonFilePath = Path.Combine(_rootFolder, $"{patchInfo.PatchVersion}.json");
        List<ChunkInfoCacheData>? dataBlockCacheInfos = null;
        string jsonText = string.Empty;
        if (File.Exists(_jsonFilePath))
        {
            jsonText = File.ReadAllText(_jsonFilePath);
        }

        if (string.IsNullOrEmpty(jsonText))
        {
            dataBlockCacheInfos = new();
        }
        else
        {
            dataBlockCacheInfos = JsonSerializer.Deserialize<List<ChunkInfoCacheData>>(jsonText)!;
        }

        foreach (var info in dataBlockCacheInfos)
        {
            _cache.Add(info.ChunkHash, info);
        }
    }

  
    public  override bool Check(string hash)
    {
        string hashString = hash;
        if (_cache.ContainsKey(hashString))
        {
            return true;
        }

        return false;
    }


    public override void Add(ChunkInfoCacheData info)
    {
        _cache.Add(info.ChunkHash, info);
    }

    public void Save()
    {
        string serialize = Serialize();
        File.WriteAllText(_jsonFilePath, serialize);
    }

    public string Serialize()
    {
        var infos = _cache.Values.ToList();
        string serialize = JsonSerializer.Serialize(infos);
        return serialize;
    }
}