using System.IO.Compression;

namespace LeagueBackupper.Core.DataBlock;

public class ZipChunkDataStorager : ChunkDataStorager, IDisposable
{
    private ZipArchive? _zipArchive;
    private string? _zipPath;
    private Stream? _zipStream;
    private readonly string _rootFolder;

    public ZipChunkDataStorager(string rootFolder)
    {
        if (!Directory.Exists(rootFolder))
        {
            Directory.CreateDirectory(rootFolder);
        }
        _rootFolder = rootFolder;
    }

    public override void Init(string patchVersion)
    {
        _zipPath = Path.Combine(_rootFolder,$"{patchVersion}.zip");
        _zipStream = File.Create(_zipPath);
        _zipArchive = new ZipArchive(_zipStream, ZipArchiveMode.Create);
    }

    public override string Write(string hash, Stream stream)
    {
        byte[] buffer = new byte[1024 * 1024];
        // string hashString = Utils.GetHashString(hash);
        ZipArchiveEntry entry = _zipArchive.CreateEntry(hash, CompressionLevel.NoCompression);
        using Stream entryStream = entry.Open();
        int readCnt = 0;
        Span<byte> buffSpan = buffer.AsSpan();
        // while ((readCnt = stream.Read(buffer, 0, buffer.Length)) > 0)
        while (true)
        {
            readCnt = stream.Read(buffSpan);
            if (readCnt == 0)
                break;
            entryStream.Write(buffer.AsSpan(0, readCnt));
        }

        entryStream.Flush();
        return _zipPath + $"/{hash}";
    }

    public override void Flush()
    {
        Dispose();
    }

    public void Dispose()
    {
        _zipArchive!.Dispose();
    }
}