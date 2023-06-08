namespace LeagueBackupper.Core.DataBlock;

public class ReadonlyZipStreamWrapper : Stream
{
    private Stream _wrappedStream;
    private Action? _disposeEvt;

    public ReadonlyZipStreamWrapper(Stream zipStream,Action? onDispose = null)
    {
        _wrappedStream = zipStream;
        _disposeEvt = onDispose;
    }

    public override void Flush()
    {
        _wrappedStream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return _wrappedStream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _wrappedStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _wrappedStream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _wrappedStream.Write(buffer, offset, count);
    }

    public override bool CanRead => _wrappedStream.CanRead;
    public override bool CanSeek => _wrappedStream.CanSeek;
    public override bool CanWrite => _wrappedStream.CanWrite;
    public override long Length => _wrappedStream.Length;

    public override long Position
    {
        get => _wrappedStream.Position;
        set => _wrappedStream.Position = value;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _wrappedStream.Dispose();
        _disposeEvt?.Invoke();
    }
}