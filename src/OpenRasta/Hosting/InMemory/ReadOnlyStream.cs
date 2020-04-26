using System;
using System.IO;

namespace OpenRasta.Hosting.InMemory
{
  public class ReadOnlyStream : Stream
  {
    readonly Stream _inner;

    public ReadOnlyStream(Stream inner)
    {
      _inner = inner;
    }

    public override void Flush()
    {
      _inner.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      return _inner.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      if (origin != SeekOrigin.Current) throw new NotSupportedException();
      return _inner.Seek(offset, origin);
    }

    public override void SetLength(long value)=> throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
    {
      _inner.Write(buffer, offset, count);
    }

    public override bool CanRead => _inner.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
      get =>  throw new NotSupportedException();
      set =>  throw new NotSupportedException();
    }
  }
}