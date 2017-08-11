using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OpenRasta.IO;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Codecs
{
    [MediaType("application/octet-stream;q=0.5")]
    [MediaType("*/*;q=0.1")]
    [SupportedType(typeof(IFile))]
    [SupportedType(typeof(Stream))]
    [SupportedType(typeof(byte[]))]
    public partial class ApplicationOctetStreamCodec :
      Codec,
      IMediaTypeReaderAsync,
      IMediaTypeReader,
      IMediaTypeWriterAsync,
      IMediaTypeWriter
    {
      public async Task<object> ReadFrom(IHttpEntity request, IType destinationType, string destinationName)
        {
            if (destinationType.IsAssignableTo<IFile>())
                return new HttpEntityFile(request);
            if (destinationType.IsAssignableTo<Stream>())
                return request.Stream;
            if (destinationType.IsAssignableTo<byte[]>())
                return await request.Stream.ReadToEndAsync();
            return Missing.Value;
        }

      public async Task WriteTo(object entity, IHttpEntity response, IEnumerable<string> codecParameters)
        {
            foreach (var writer in GetWritersAsync(entity, response))
                if (await writer)
                    return;

            throw new InvalidOperationException();
        }


        static async Task<bool> TryProcessAsAsync<T>(object target, Func<T, Task> action) where T : class
        {
            var typedTarget = target as T;
            if (typedTarget == null) return false;
            await action(typedTarget);
            return true;
        }

      static async Task WriteFileWithFilenameAsync(IFile file, string disposition, IHttpEntity response)
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
                await stream.CopyToAsync(response.Stream);
        }

      IEnumerable<Task<bool>> GetWritersAsync(object entity, IHttpEntity response)
        {
            yield return TryProcessAsAsync<IDownloadableFile>(entity,
                file => WriteFileWithFilenameAsync(file, "attachment", response));
            yield return TryProcessAsAsync<IFile>(entity, file => WriteFileWithFilenameAsync(file, "inline", response));
            // TODO: Stream to be disposed and length to be written if needed
            yield return TryProcessAsAsync<Stream>(entity, stream => stream.CopyToAsync(response.Stream));
            yield return TryProcessAsAsync<byte[]>(entity, bytes => response.Stream.WriteAsync(bytes, 0, bytes.Length));
        }
    }
}
