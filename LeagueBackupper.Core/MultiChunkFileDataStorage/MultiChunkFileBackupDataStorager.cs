using System.Security.Cryptography;
using LeagueBackupper.Common.Utils;
using LeagueBackupper.Core.DataBlock;
using LeagueBackupper.Core.MultiChunkFile;
using LeagueBackupper.Core.Pipeline.MultiChunkFileDataStorage;
using LeagueBackupper.Core.Structure;
using LeagueToolkit.Core.Wad;
using SubstreamSharp;

namespace LeagueBackupper.Core.MultiChunkFileDataStorage;

public class DefaultPathDataStorager : PatchFileDataStorager
{
    private const string DataFolder = "Data";
    private const string MultiDataBlockPatchFileInfoFolder = "PatchFiles";
    private const string VersionFilesFilePath = "VersionFiles.json";
    private readonly ChunkDataStorager _chunkDataStorager;
    private readonly ChunkExistChecker _chunkExistChecker;
    private readonly PatchMultiChunkFileInfoManager _patchMultiChunkFileInfoManager;
    private readonly MD5 _md5;
    private List<MultiChunkFileInfo> _fileInfos = null!;
    private PatchInfo _curPatchInfo = null!;

    public DefaultPathDataStorager(
        ChunkDataStorager chunkDataStorager,
        ChunkExistChecker chunkExistChecker,
        PatchMultiChunkFileInfoManager patchMultiChunkFileInfoManager)
    {
        _md5 = MD5.Create();
        _chunkDataStorager = chunkDataStorager;
        _chunkExistChecker = chunkExistChecker;
        _patchMultiChunkFileInfoManager = patchMultiChunkFileInfoManager;
    }

    public override void Init(PatchInfo patchInfo)
    {
        _curPatchInfo = patchInfo;
        _chunkDataStorager.Init(patchInfo.PatchVersion);
        _fileInfos = new();
    }

    public override void WritePatchFile(PatchFileInfo pf, Stream clientFileStream)
    {
        // MultiDataBlockFile? exists = _patchStorageManager.GetMultiDataBlockFile(pf.Hash);
        // if (exists != null)
        // {
        //     return;
        // }

        MultiChunkFileInfo multiChunkFileInfo = new MultiChunkFileInfo();
        multiChunkFileInfo.Length = pf.Length;
        multiChunkFileInfo.FileHash = pf.Hash;
        // string extension = Path.GetExtension(pf.Path);
        var endWithWadClient = pf.Filename.EndsWith(".wad.client");
        if (endWithWadClient)
        {
            var fileInfo = ProcessWadFile(pf, clientFileStream);
            _fileInfos.Add(fileInfo);
        }
        else
        {
            var fileInfo = ProcessOtherFile(pf, clientFileStream);
            _fileInfos.Add(fileInfo);
        }
    }

    public override void Complete()
    {
        _chunkDataStorager.Flush();
        _patchMultiChunkFileInfoManager.Save(_curPatchInfo.PatchVersion, _fileInfos);
    }

    #region Private Method

    class IEC : IEqualityComparer<(long, int)>
    {
        public bool Equals((long, int) x, (long, int) y)
        {
            return x.Item1 == y.Item1 && x.Item2 == y.Item2;
        }

        public int GetHashCode((long, int) obj)
        {
            return HashCode.Combine(obj.Item1, obj.Item2);
        }
    }

    private MultiChunkFileInfo ProcessWadFile(PatchFileInfo vf, Stream stream)
    {
        WadFile wadFile = new WadFile(stream);
        List<(long, int)> chunksSizeInfo = new List<(long, int)>();

        foreach (var chunk in wadFile.Chunks.Values)
        {
            long chunkDataOffset = chunk.DataOffset;
            int chunkCompressedSize = chunk.CompressedSize;
            chunksSizeInfo.Add((chunkDataOffset, chunkCompressedSize));
        }

        chunksSizeInfo = chunksSizeInfo.Distinct(new IEC()).ToList();
        List<(long offset, long len)> dataBlockSegmentDef = new();
        chunksSizeInfo.Sort((left, right) => { return (int)(left.Item1 - right.Item1); });
        for (var index = 0; index < chunksSizeInfo.Count; index++)
        {
            var (offset, len) = chunksSizeInfo[index];
            if (index == chunksSizeInfo.Count - 1)
            {
                break;
            }

            if (offset + len > chunksSizeInfo[index + 1].Item1)
            {
                if (offset != chunksSizeInfo[index + 1].Item1)
                {
                    throw new Exception("Fatal Error!!!! chunks offset overlapped!!!");
                }
            }
        }

        if (chunksSizeInfo.Count == 0)
        {
            dataBlockSegmentDef.Add((0, vf.Length));
        }
        else
        {
            var (firstOffset, firstLen) = chunksSizeInfo[0];
            var (lastOffset, lastLen) = chunksSizeInfo.Last();
            if (firstOffset != 0)
            {
                dataBlockSegmentDef.Add((0, firstOffset));
            }

            //generate data block for all chunks in this wad file.
            var count = chunksSizeInfo.Count;
            for (var index = 0; index < count; index++)
            {
                var (offset, len) = chunksSizeInfo[index];
                dataBlockSegmentDef.Add((offset, len));
                if (index < count - 1)
                {
                    var (nextChunkOffset, nextChunkLen) = chunksSizeInfo[index + 1];
                    var dataLenBetweenTwoChunks = nextChunkOffset - offset - len;
                    if (dataLenBetweenTwoChunks > 0)
                    {
                        dataBlockSegmentDef.Add((offset + len, dataLenBetweenTwoChunks));
                    }
                }
            }

            var lastSegmentPos = lastOffset + lastLen;
            var lastSegmentLen = vf.Length - lastSegmentPos;
            if (lastSegmentLen != 0)
            {
                dataBlockSegmentDef.Add((lastSegmentPos, lastSegmentLen));
            }
        }

        List<ChunkInfo> dataBlockChunks = new List<ChunkInfo>();
        for (int i = 0; i < dataBlockSegmentDef.Count; i++)
        {
            var (offset, len) = dataBlockSegmentDef[i];
            Substream dbStream = new Substream(stream, offset, len);
            byte[] dbHash = _md5.ComputeHash(dbStream);
            string hashStr = Utils.GetHashString(dbHash);
            dataBlockChunks.Add(new ChunkInfo()
            {
                Offset = offset,
                Hash = hashStr,
                Length = len
            });
            if (!_chunkExistChecker.Check(hashStr))
            {
                dbStream.Seek(0, SeekOrigin.Begin);
                string url = _chunkDataStorager.Write(hashStr, dbStream);
                _chunkExistChecker.Add(new ChunkInfoCacheData() { ChunkHash = hashStr, Url = url });
            }
        }

        MultiChunkFileInfo result = new MultiChunkFileInfo();
        result.FileHash = vf.Hash;
        result.Length = vf.Length;
        result.Chunks = dataBlockChunks;
        return result;
    }

    private MultiChunkFileInfo ProcessOtherFile(PatchFileInfo vf, Stream stream)
    {
        var dataBlockInfos = new List<ChunkInfo>();
        var dataBlockInfo = new ChunkInfo()
        {
            Offset = 0, Hash = vf.Hash, Length = vf.Length
        };
        if (!_chunkExistChecker.Check(dataBlockInfo.Hash))
        {
            stream.Seek(0, SeekOrigin.Begin);
            string url = _chunkDataStorager.Write(dataBlockInfo.Hash, stream);
            _chunkExistChecker.Add(new ChunkInfoCacheData() { ChunkHash = dataBlockInfo.Hash, Url = url });
        }

        dataBlockInfos.Add(dataBlockInfo);
        MultiChunkFileInfo result = new MultiChunkFileInfo();
        result.Chunks = dataBlockInfos;
        result.FileHash = dataBlockInfo.Hash;
        result.Length = vf.Length;
        return result;
    }

    #endregion
}