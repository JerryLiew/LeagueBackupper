using System.Text.Json;
using System.Text.Json.Serialization;
using LeagueBackupper.Core.Structure;

namespace LeagueBackupper.Core;

class Converter : JsonConverter<byte[]>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return true;
    }

    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
        // reader.TokenType
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value);
    }
}

public class JsonPatchManager : PatchManager
{
    private readonly string _patchStorageFolder;

    public JsonPatchManager(string patchStorageFolder)
    {
        _patchStorageFolder = patchStorageFolder;
        if (!Directory.Exists(patchStorageFolder))
        {
            Log.Warn("The patch storage folder does not exists, Create it automatically.");
            Directory.CreateDirectory(patchStorageFolder);
        }
    }


    public override PatchInfo GetPatchInfo(string patchVersion)
    {
        if (!ContainsPatch(patchVersion))
        {
            throw new PatchNotFoundException($"The client version:{patchVersion} not found.");
        }

        var versionDeserialized = DeserializeLeagueVersionFromFile(patchVersion)!;
        return versionDeserialized;
    }

    public override void RecordPatch(PatchInfo version)
    {
        Log.Err("序列化");
        string versionJson = SerializeLeagueVersion(version);
        Log.Err($"序列化结果{versionJson}");
        File.WriteAllText(Path.Combine(_patchStorageFolder, version.PatchVersion + ".json"), versionJson);
    }

    public override bool ContainsPatch(string patchVersion)
    {
        List<string> clientVersions = GetAllPatchesVersion();
        string? find = clientVersions.Find(s => s == patchVersion);
        return find != null;
    }

    public override List<string> GetAllPatchesVersion()
    {
        List<string> result = new List<string>();
        if (!Directory.Exists(_patchStorageFolder))
        {
            return result;
        }

        DirectoryInfo directoryInfo = new DirectoryInfo(_patchStorageFolder);
        FileInfo[] fileInfos = directoryInfo.GetFiles();
        foreach (var info in fileInfos)
        {
            result.Add(Path.GetFileNameWithoutExtension(info.Name));
        }

        return result;
    }


    #region Private Method

    private PatchInfo? DeserializeLeagueVersionFromFile(string clientVersion)
    {
        string versionFile = Path.Combine(_patchStorageFolder, clientVersion + ".json");
        using var fs = File.OpenRead(versionFile);
        PatchInfo? leagueVersion = JsonSerializer.Deserialize<PatchInfo>(fs);
        return leagueVersion;
    }

    private string SerializeLeagueVersion(PatchInfo version)
    {
        string serialize = JsonSerializer.Serialize(version);
        return serialize;
    }

    #endregion
}