using System;
using System.IO;
using System.Net;
using System.Text;
using OpenRasta.Configuration;
using OpenRasta.IO;
using Shouldly;
using Xunit;

namespace Tests.Hosting.HttpListener
{
  public class serving_files : IDisposable
  {
    readonly TestHttpListener _httpListener;

    public serving_files()
    {
      _httpListener = new TestHttpListener(new Configuration());
    }

    [Fact]
    public void can_get_without_length() => _httpListener.WebGet("file-no-length").ShouldBe("hello world");

    [Fact]
    public void can_get_with_length() => _httpListener.WebGet("file-with-length").ShouldBe("hello world");

    class Configuration : IConfigurationSource
    {
      public void Configure()
      {
        ResourceSpace.Has.ResourcesNamed("File1").AtUri("/file-no-length").HandledBy<NoLengthHandler>();
        ResourceSpace.Has.ResourcesNamed("File2").AtUri("/file-with-length").HandledBy<WithLengthHandler>();
      }
    }

    class NoLengthHandler
    {
      readonly byte[] Bytes = Encoding.UTF8.GetBytes("hello world");

      public IFile Get() => new InMemoryFile(new MemoryStream(Bytes));
    }

    class WithLengthHandler
    {
      readonly byte[] Bytes = Encoding.UTF8.GetBytes("hello world");

      public IFile Get() => new InMemoryFile(new MemoryStream(Bytes))
      {
          Length = Bytes.Length
      };
    }

    public void Dispose()
    {
      _httpListener.Host.Close();
    }
  }
}