using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Web;

namespace OpenRasta.Hosting.AspNet.AspNetHttpListener
{
  public class HttpListenerWorkerRequest : HttpWorkerRequest
  {
    readonly HttpListenerContext _context;
    readonly string _physicalDir;
    readonly string _virtualDir;

    public HttpListenerWorkerRequest(
      HttpListenerContext context, string vdir, string pdir)
    {
      if (null == vdir || vdir.Equals(string.Empty))
        throw new ArgumentException("vdir");
      if (null == pdir || pdir.Equals(string.Empty))
        throw new ArgumentException("pdir");

      _context = context ?? throw new ArgumentNullException(nameof(context));
      _virtualDir = vdir;
      _physicalDir = pdir;
      _context.Response.SendChunked = false;
    }

    public override void CloseConnection()
    {
    }

    public override void EndOfRequest()
    {
      _context.Response.Close();
    }

    public override void FlushResponse(bool finalFlush)
    {
      _context.Response.OutputStream.Flush();
    }

    public override string GetAppPath()
    {
      return _virtualDir;
    }

    public override string GetAppPathTranslated()
    {
      return _physicalDir;
    }

    public override string GetFilePath()
    {
      return _context.Request.Url.LocalPath;
    }

    public override string GetFilePathTranslated()
    {
      var s = GetFilePath();
      s = s.Substring(_virtualDir.Length);
      s = s.Replace('/', Path.DirectorySeparatorChar);
      return Path.Combine(_physicalDir, s);
    }

    public override string GetHttpVerbName()
    {
      return _context.Request.HttpMethod;
    }

    public override string GetHttpVersion()
    {
      return $"HTTP/{_context.Request.ProtocolVersion.Major}.{_context.Request.ProtocolVersion.Minor}";
    }

    public override string MapPath(string virtualPath)
    {
      var fsPath = virtualPath.Replace('/', Path.DirectorySeparatorChar);
      fsPath = fsPath[0] == Path.DirectorySeparatorChar ? fsPath.Substring(1) : fsPath;
      var mapped = Path.Combine(
        _physicalDir,
        fsPath);
      return mapped;
    }

    public override string GetKnownRequestHeader(int index)
    {
      switch (index)
      {
        case HeaderUserAgent:
          return _context.Request.UserAgent;
        default:
          return _context.Request.Headers[GetKnownRequestHeaderName(index)];
      }
    }

    public override string GetLocalAddress()
    {
      // ReSharper disable once PossibleNullReferenceException
      return _context.Request.LocalEndPoint?.Address.ToString();
    }

    public override int GetLocalPort()
    {
      // ReSharper disable once PossibleNullReferenceException
      return _context.Request.LocalEndPoint.Port;
    }

    public override string GetPathInfo()
    {
      var s1 = GetFilePath();
      var s2 = _context.Request.Url.LocalPath;
      if (s1.Length == s2.Length)
        return string.Empty;
      return s2.Substring(s1.Length);
    }

    public override string GetQueryString()
    {
      var queryString = string.Empty;
      var rawUrl = _context.Request.RawUrl;
      var index = rawUrl.IndexOf('?');
      if (index != -1)
        queryString = rawUrl.Substring(index + 1);
      return queryString;
    }

    public override string GetRawUrl()
    {
      return _context.Request.RawUrl;
    }

    public override string GetRemoteAddress()
    {
      return _context.Request.RemoteEndPoint.Address.ToString();
    }

    public override int GetRemotePort()
    {
      return _context.Request.RemoteEndPoint.Port;
    }

    public override string GetServerVariable(string name)
    {
      // TODO: vet this list
      switch (name)
      {
        case "HTTPS":
          return _context.Request.IsSecureConnection ? "on" : "off";
        case "HTTP_USER_AGENT":
          return _context.Request.Headers["UserAgent"];
        case "HTTP_HOST":
          return _context.Request.Headers["Host"];
        default:
          return null;
      }
    }

    public override string GetUnknownRequestHeader(string name)
    {
      return _context.Request.Headers[name];
    }

    public override string[][] GetUnknownRequestHeaders()
    {
      var headers = _context.Request.Headers;
      var count = headers.Count;
      var headerPairs = new List<string[]>(count);
      for (var i = 0; i < count; i++)
      {
        var headerName = headers.GetKey(i);
        if (GetKnownRequestHeaderIndex(headerName) != -1) continue;
        var headerValue = headers.Get(i);
        headerPairs.Add(new[] {headerName, headerValue});
      }
      return headerPairs.ToArray();
    }

    public override string GetUriPath()
    {
      return _context.Request.Url.LocalPath;
    }

    public override int ReadEntityBody(byte[] buffer, int size)
    {
      return _context.Request.InputStream.Read(buffer, 0, size);
    }

    public override void SendKnownResponseHeader(int index, string value)
    {
      if (GetKnownRequestHeaderName(index) == "Content-Length")
      {
        _context.Response.ContentLength64 = long.Parse(value, CultureInfo.InvariantCulture);
        return;
      }
      try
      {
        _context.Response.Headers[
          GetKnownResponseHeaderName(index)] = value;
      }
      catch
      {
        Debug.WriteLine(string.Empty);
      }
    }

    public override void SendResponseFromFile(
      IntPtr handle, long offset, long length)
    {
      Debug.WriteLine(string.Empty);
    }

    public override void SendResponseFromFile(
      string filename, long offset, long length)
    {
      Debug.WriteLine(string.Empty);
    }

    public override void SendResponseFromMemory(byte[] data, int length)
    {
      _context.Response.OutputStream.Write(data, 0, length);
    }

    public override void SendStatus(int statusCode, string statusDescription)
    {
      _context.Response.StatusCode = statusCode;
      _context.Response.StatusDescription = statusDescription;
    }

    public override void SendUnknownResponseHeader(string name, string value)
    {
      _context.Response.Headers[name] = value;
    }
  }
}