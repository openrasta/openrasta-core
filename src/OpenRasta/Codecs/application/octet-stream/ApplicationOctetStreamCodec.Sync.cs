using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenRasta.IO;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Codecs
{
  public partial class ApplicationOctetStreamCodec
  {
    object IMediaTypeReader.ReadFrom(IHttpEntity request, IType destinationType, string destinationName)
    {
      if (destinationType.IsAssignableTo<IFile>())
        return new HttpEntityFile(request);
      if (destinationType.IsAssignableTo<Stream>())
        return request.Stream;
      if (destinationType.IsAssignableTo<byte[]>())
        return request.Stream.ReadToEnd();
      return Missing.Value;
    }

    void IMediaTypeWriter.WriteTo(object entity, IHttpEntity response, string[] codecParameters)
    {
      if (!GetWriters(entity, response).Any(x => x))
        throw new InvalidOperationException();
    }

    private static bool TryProcessAs<T>(object target, Action<T> action) where T : class
    {
      var typedTarget = target as T;
      if (typedTarget == null) return false;
      action(typedTarget);
      return true;
    }

    private static void WriteFileWithFilename(IFile file, string disposition, IHttpEntity response)
    {
      var contentDispositionHeader =
        response.Headers.ContentDisposition ?? new ContentDispositionHeader(disposition);

      if (!string.IsNullOrEmpty(file.FileName))
        contentDispositionHeader.FileName = file.FileName;
      if (!string.IsNullOrEmpty(contentDispositionHeader.FileName) ||
          contentDispositionHeader.Disposition != "inline")
        response.Headers.ContentDisposition = contentDispositionHeader;
      if (file.ContentType != null && file.ContentType != MediaType.ApplicationOctetStream
          || (file.ContentType == MediaType.ApplicationOctetStream && response.ContentType == null))
        response.ContentType = file.ContentType;
      // TODO: use contentLength from IFile
      using (var stream = file.OpenStream())
        stream.CopyTo(response.Stream);
    }

    private IEnumerable<bool> GetWriters(object entity, IHttpEntity response)
    {
      yield return TryProcessAs<IDownloadableFile>(entity,
        file => WriteFileWithFilename(file, "attachment", response));
      yield return TryProcessAs<IFile>(entity, file => WriteFileWithFilename(file, "inline", response));
      // TODO: Stream to be disposed and length to be written if needed
      yield return TryProcessAs<Stream>(entity, stream => stream.CopyTo(response.Stream));
      yield return TryProcessAs<byte[]>(entity, bytes => response.Stream.Write(bytes, 0, bytes.Length));
    }
  }
}