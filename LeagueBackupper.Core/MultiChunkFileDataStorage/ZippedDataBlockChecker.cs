using System.IO.Compression;
using LeagueBackupper.Core.MultiChunkFileDataStorage.DataBlock;
using LeagueBackupper.Core.Pipeline.MultiChunkFileDataStorage;
using LeagueBackupper.Core.Structure;

namespace LeagueBackupper.Core.MultiChunkFileDataStorage;

public class ZippedDataBlockChecker : ChunkExistChecker
{
    private HashSet<string> _hashSet = new();
    private List<string> _zipFiles = new();

    public ZippedDataBlockChecker WithZipFilesFolder(string folder)
    {
        if (!Directory.Exists(folder))
        {
            return this;
        }

        DirectoryInfo di = new DirectoryInfo(folder);
        IEnumerable<FileInfo> enumerateFiles = di.EnumerateFiles();
        foreach (var fi in enumerateFiles)
        {
            _zipFiles.Add(fi.FullName);
        }

        return this;
    }

    public ZippedDataBlockChecker WithZipFile(string zipFile)
    {
        _zipFiles.Add(zipFile);
        return this;
    }

    public override void Init(PatchInfo patchInfo)
    {
        foreach (var zipFile in _zipFiles)
        {
            using FileStream fileStream = File.OpenRead(zipFile);
            using ZipArchive archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
            var zipArchiveEntries = archive.Entries;
            foreach (var entry in zipArchiveEntries)
            {
                string entryName = entry.Name;
                bool add = _hashSet.Add(entryName);
            }
        }
    }

    public override bool Check(string hash)
    {
        return _hashSet.Contains(hash);
    }

    public override void Add(ChunkInfoCacheData info)
    {
        _hashSet.Add(info.ChunkHash);
    }
}