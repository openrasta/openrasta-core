using System.IO;
using System.Linq;

namespace OpenRasta.Hosting.Katana
{
  public class DelayedStream : Stream
  {
    readonly Stream _baseStream;
    readonly MemoryStream _delayedStream;
    byte[] _bytes;

    public DelayedStream(Stream baseStream)
    {
      _baseStream = baseStream;
      _delayedStream = new MemoryStream();
    }

    public override bool CanRead => _delayedStream.CanRead;

    public override bool CanSeek => _delayedStream.CanSeek;

    public override bool CanWrite => _delayedStream.CanWrite;

    public override long Length => _delayedStream.Length;

    public override long Position
    {
      get => _delayedStream.Position;
      set => _delayedStream.Position = value;
    }

    public override void Flush()
    {
      if (_bytes != null) _baseStream.Write(_bytes, offset: 0, count: _bytes.Count());
      _baseStream.Flush();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      return _delayedStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
      _delayedStream.SetLength(value);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      return _delayedStream.Read(buffer, offset, count);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      if (_bytes == null)
        _bytes = buffer;
      else
        _bytes.CopyTo(buffer, index: 0);
      _delayedStream.Write(buffer, offset, count);
    }
  }
}