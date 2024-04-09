using System.Data;
using LeagueBackupper.Core.MultiChunkFileDataStorage.DataBlock;

namespace LeagueBackupper.Core.DataBlock;

public class ZipFilesChunkDataProvider : ChunkDataProvider
{
    private Dictionary<string, (DataBlockInfo, string)> _zipFilePathMap = new();
    private List<string> _zipFiles = new();
    private Dictionary<string, ZipChunkDataProvider> _providerCache = new();

    public ZipFilesChunkDataProvider()
    {
    }

    public ZipFilesChunkDataProvider WithZipFiles(List<string> zipFilesName)
    {
        foreach (var file in zipFilesName)
        {
            WithZipFile(file);
        }

        return this;
    }

    public ZipFilesChunkDataProvider WithZipFile(string zipFileName)
    {
        if (_zipFiles.Contains(zipFileName))
        {
            throw new DuplicateNameException($"The zip file:{zipFileName} already exist.");
        }

        _zipFiles.Add(zipFileName);
        return this;
    }

    public ZipFilesChunkDataProvider WithFolder(string folderName)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(folderName);
        FileInfo[] fileInfos = directoryInfo.GetFiles();
        foreach (var fi in fileInfos)
        {
            WithZipFile(fi.FullName);
        }

        return this;
    }

    public override void Init(string patchVersion)
    {
        base.Init(patchVersion);
        Load();
    }

    private void Load()
    {
        foreach (var file in _zipFiles)
        {
            using var zipProvider = new ZipChunkDataProvider(file);
            var allDataBlockDescriptors = zipProvider.GetAllDataBlockInfo();
            foreach (var dataBlockDescriptor in allDataBlockDescriptors)
            {
                string hashString = dataBlockDescriptor.Hash;
                _zipFilePathMap.Add(hashString, (dataBlockDescriptor, file));
            }
        }
    }

    public override Stream OpenRead(string hash)
    {
        string path = _zipFilePathMap[hash].Item2;
        if (!_providerCache.TryGetValue(path, out var provider))
        {
            provider = new ZipChunkDataProvider(path);
            _providerCache.Add(path, provider);
        }

        Stream stream = provider.OpenRead(hash);
        var zipReadonlyStreamWrapper = new ReadonlyZipStreamWrapper(stream, () =>
        {
            /* provider.Dispose();*/
        });
        return zipReadonlyStreamWrapper;
    }

    public override bool TryGetDataBlockInfo(string hash, out DataBlockInfo? dataBlockInfo)
    {
        bool tryGetValue = _zipFilePathMap.TryGetValue(hash, out var valueTuple);
        dataBlockInfo = valueTuple.Item1;
        return tryGetValue;
    }


    public override List<DataBlockInfo> GetAllDataBlockInfo()
    {
        return _zipFilePathMap.Values.Select((tuple, i) => tuple.Item1).ToList();
    }

    public void Dispose()
    {
    }
}