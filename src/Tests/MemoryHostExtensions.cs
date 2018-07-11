using System;
using System.Text;
using System.Threading.Tasks;
using OpenRasta.Hosting.InMemory;
using OpenRasta.IO;
using OpenRasta.Web;

namespace Tests
{
  public static class MemoryHostExtensions
  {
    public static Task<IResponse> Post(this InMemoryHost host, string uri, string content, string contentType = null)
    {
      return ExecuteMethod(host, "POST", uri, content, contentType);
    }

    public static Task<IResponse> Put(this InMemoryHost host, string uri, string content, string contentType = null)
    {
      return ExecuteMethod(host, "PUT", uri, content, contentType);
    }

    public static Task<IResponse> Get(this InMemoryHost host, string uri, string accept = null)
    {
      return ExecuteMethod(host, uri, "GET", headers: h => h["Accept"] = accept);
    }

    static Task<IResponse> ExecuteMethod(InMemoryHost host, string uri, string method, string content = null,
      string contentType = null, Action<HttpHeaderDictionary> headers = null)
    {
      var request = new InMemoryRequest
      {
        HttpMethod = method,
        Uri = new Uri($"http://localhost{uri}", UriKind.RelativeOrAbsolute)
      };
      headers?.Invoke(request.Headers);
      if (content != null) request = request.WriteString(content, contentType);
      return host.ProcessRequestAsync(request);
    }

    public static string ReadString(this IResponse response)
    {
      return Encoding.UTF8.GetString(response.Entity.Stream.ReadToEnd());
    }

    static InMemoryRequest WriteString(this InMemoryRequest request, string text, string contentType = null)
    {
      request.Entity.ContentType = new MediaType(contentType ?? "text/plain;charset=utf-8");

      var buffer = Encoding.UTF8.GetBytes(text);
      request.Entity.ContentLength = buffer.Length;
      request.Entity.Stream.Write(buffer);
      return request;
    }
  }
}