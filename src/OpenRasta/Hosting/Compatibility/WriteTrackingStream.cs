using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenRasta.Hosting.Compatibility
{
  public class WriteTrackingStream : Stream
  {
    readonly Stream _innerStream;
    readonly WriteTrackingResponse _response;

    public WriteTrackingStream(Stream innerStream,
      WriteTrackingResponse response)
    {
      _innerStream = innerStream;
      _response = response;
    }

    public override void Flush()
    {
      _response.WriteHeaders();
      _innerStream.Flush();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      return _innerStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
      _innerStream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      _response.WriteHeaders();
      _innerStream.Write(buffer, offset, count);
    }

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
    {
      _response.WriteHeaders();
      return base.BeginWrite(buffer, offset, count, callback, state);
    }

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
      _response.WriteHeaders();
      return base.CopyToAsync(destination, bufferSize, cancellationToken);
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
      _response.WriteHeaders();
      return base.FlushAsync(cancellationToken);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      return _innerStream.Read(buffer, offset, count);
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
      _response.WriteHeaders();
      return base.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override void WriteByte(byte value)
    {
      _response.WriteHeaders();
      base.WriteByte(value);
    }

    public override bool CanRead => _innerStream.CanRead;

    public override bool CanSeek => _innerStream.CanSeek;

    public override bool CanWrite => _innerStream.CanWrite;

    public override long Length => _innerStream.Length;

    public override long Position
    {
      get => _innerStream.Position;
      set => _innerStream.Position = value;
    }
  }
}