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

    public WriteTrackingStream(
      Stream innerStream,
      WriteTrackingResponse response)
    {
      _innerStream = innerStream;
      _response = response;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      return _innerStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
      _innerStream.SetLength(value);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      return _innerStream.Read(buffer, offset, count);
    }
    
    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
    {
      return _innerStream.BeginRead(buffer, offset, count, callback, state);
    }

    public override bool CanTimeout => _innerStream.CanTimeout;

    public override void Close()
    {
      // don't let consumer code close response streams
      // TODO: Add flag to enable stricter control than just ignoring shit
//      throw new IOException("A response stream is private to its server, don't go closing it!");
    }

    protected override void Dispose(bool disposing)
    {
      // TODO: Add flag to enable stricter control than just ignoring shit
//      if (disposing) throw new IOException("A response stream is private to its server, don't go closing it!");

      base.Dispose(false);
    }

    public override int EndRead(IAsyncResult asyncResult)
    {
      return _innerStream.EndRead(asyncResult);
    }

    public override void EndWrite(IAsyncResult asyncResult)
    {
      _innerStream.EndWrite(asyncResult);
    }

    public override bool Equals(object obj)
    {
      return _innerStream.Equals(obj);
    }

    public override int GetHashCode()
    {
      return _innerStream.GetHashCode();
    }

    public override object InitializeLifetimeService()
    {
      return _innerStream.InitializeLifetimeService();
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
      return _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override int ReadByte()
    {
      return _innerStream.ReadByte();
    }

    public override int ReadTimeout
    {
      get => _innerStream.ReadTimeout;
      set => _innerStream.ReadTimeout = value;
    }

    public override string ToString()
    {
      return _innerStream.ToString();
    }

    public override int WriteTimeout
    {
      get => _innerStream.WriteTimeout;
      set => _innerStream.WriteTimeout = value;
    }

    public override void Flush()
    {
      EnsureHeadersSent();
      _innerStream.FlushAsync();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      EnsureHeadersSent();
      _innerStream.WriteAsync(buffer, offset, count);
    }

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
    {
      EnsureHeadersSent();
      return _innerStream.BeginWrite(buffer, offset, count, callback, state);
    }

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
      EnsureHeadersSent();
      return _innerStream.CopyToAsync(destination, bufferSize, cancellationToken);
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
      EnsureHeadersSent();
      return _innerStream.FlushAsync(cancellationToken);
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
      EnsureHeadersSent();
      return _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override void WriteByte(byte value)
    {
      EnsureHeadersSent();
      _innerStream.WriteByte(value);
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

    void EnsureHeadersSent()
    {
      if (_response.HeadersSent == false) _response.WriteHeaders();
    }
  }
}