using System.IO.Compression;
using LeagueBackupper.Core.MultiChunkFileDataStorage.DataBlock;

namespace LeagueBackupper.Core.DataBlock;

public class ZipChunkDataProvider : ChunkDataProvider, IDisposable
{
    private readonly ZipArchive _zipArchive;
    private string _zipPath;
    private Stream _zipStream;

    public ZipChunkDataProvider(string zipPath)
    {
        _zipPath = zipPath;
        _zipStream = File.Open(zipPath, FileMode.Open);
        _zipArchive = new ZipArchive(_zipStream, ZipArchiveMode.Read);
    }

    public void Dispose()
    {
        _zipStream.Dispose();
        _zipArchive.Dispose();
    }

    public override  Stream OpenRead(string hash)
    {
        // string hashString = Utils.GetHashString(hash);
        ZipArchiveEntry zipArchiveEntry = _zipArchive.GetEntry(hash)!;
        Stream stream = zipArchiveEntry.Open();
        return stream;
    }


    public override bool TryGetDataBlockInfo(string hash, out DataBlockInfo? dataBlockInfo)
    {
        var entry = _zipArchive.GetEntry(hash);
        if (entry == null)
        {
            dataBlockInfo = null;
            return false;
        }

        long entryCompressedLength = entry.CompressedLength;
        dataBlockInfo = new DataBlockInfo(hash, entryCompressedLength);
        return true;
    }

    public override List<DataBlockInfo> GetAllDataBlockInfo()
    {
        List<DataBlockInfo> result = new List<DataBlockInfo>();
        var zipArchiveEntries = _zipArchive.Entries;
        foreach (var entry in zipArchiveEntries)
        {
            // byte[] hash = Utils.GetBytesFromHashString(entry.Name);
            result.Add(new DataBlockInfo(entry.Name, entry.CompressedLength));
        }

        return result;
    }
}