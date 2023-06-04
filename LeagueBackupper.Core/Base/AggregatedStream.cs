namespace LeagueBackupper.Core;

public class AggregatedStream : Stream
{
    
    private readonly List<Stream> _subStreams;

    public AggregatedStream(List<Stream> subStreams)
    {
        _subStreams = subStreams;
    }

    public override void Flush()
    {
        throw new NotSupportedException("Operation flush not supported.");
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int totalReadCnt = 0;
        while (true)
        {
            // Calculate which sub stream to read data from
            long sumStreamLen = 0;
            Stream? curSubStream = null;
            foreach (var subSteam in _subStreams)
            {
                sumStreamLen += subSteam.Length;
                if (sumStreamLen > Position)
                {
                    curSubStream = subSteam;
                    break;
                }
            }

            if (curSubStream == null)
            {
                return totalReadCnt;
            }

            int startPosInCurSubStream = (int)(curSubStream.Length - (sumStreamLen - Position));
            int readCnt = 0;
            // curSubStream.Seek(startPosInCurSubStream, SeekOrigin.Begin);
            while ((readCnt = curSubStream.Read(buffer, offset, count)) > 0)
            {
                Position += readCnt;
                totalReadCnt += readCnt;
                offset += readCnt;
                count -= readCnt;
                if (count == 0)
                {
                    //读取满了
                    return totalReadCnt;
                }
                else
                {
                    //还未读取满
                    break;
                }
            }
        }
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        if (origin != SeekOrigin.Begin)
        {
            throw new NotSupportedException($"Operation Seek with origin type:{origin} not supported.");
        }

        Position = offset;
        return Position;
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException("Operation SetLength not supported.");
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException("Operation Write not supported.");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (var subSteam in _subStreams)
        {
            subSteam.Dispose();
        }
    }

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length => _subStreams.Sum(stream => stream.Length);
    public override long Position { get; set; } = 0;
}