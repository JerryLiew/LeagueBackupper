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
        _chunkExistChecker.Init(patchInfo);
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
        chunksSizeInfo.Sort((left, right) =>
        {
            //确保,当offset一样时, len小的在前面,具体原因看下面的note
            if (left.Item1 == right.Item1)
            {
                return left.Item2 - right.Item2;
            }

            return (int)(left.Item1 - right.Item1);
        });
        // for (var index = 0; index < chunksSizeInfo.Count; index++)
        // {
        //     var (offset, len) = chunksSizeInfo[index];
        //     if (index == chunksSizeInfo.Count - 1)
        //     {
        //         break;
        //     }
        //
        //     var next = chunksSizeInfo[index + 1];
        //     if (offset + len > next.Item1)
        //     {
        //         if (offset != next.Item1)
        //         {
        //             Console.WriteLine("Fatal Error!!!! chunks offset overlapped!!!");
        //         }
        //     }
        // }

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
                    //note:前面虽然根据offset和len进行了去重,但是有时候, 会有重复的chunk块,且两个chunk块长度不一致  例如:  连续的两个 chunk1: offset: 20 ,len: 15 ,  chunk2: offset: 20 ,len: 0(空文件). ,然后就会导致, 计算chunk2 与 chunk3 之前的gap数据时, 
                    // nextChunkOffset - offset - len 这里的len是0, 从而得到错误的结果大于0.  但是实际上应该用chunk1的offset和 len计算.
                    //  解决方案是, 排序时, 先根据offset排序, 然后根据len排序.使得长的一定在后面. 从而不会错误的添加gap数据
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