using System;
using System.IO;
using OpenRasta.IO;

namespace OpenRasta.Web
{
    public class HttpEntityFile : IFile
    {
        private bool _disposed;

        private IHttpEntity _entity;

        public HttpEntityFile(IHttpEntity entity)
        {
            _entity = entity;
        }

        public MediaType ContentType
        {
            get { return _entity.ContentType ?? MediaType.ApplicationOctetStream; }
        }

        public string FileName
        {
            get { return _entity.Headers.ContentDisposition != null ? _entity.Headers.ContentDisposition.FileName : null; }
        }

        public long Length
        {
            get { return _entity.Stream.Length; }
        }

        public Stream OpenStream()
        {
            return _entity.Stream;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                if (_entity != null)
                {
                    _entity.Dispose();
                    _entity = null;
                }
            }
            _disposed = true;
        }
    }
}