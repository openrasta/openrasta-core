using System;
using System.Net;
using OpenRasta.Configuration;
using OpenRasta.Hosting.HttpListener;

namespace Tests
{
  public class TestHttpListener
  {
    static readonly Random Random = new Random();

    public TestHttpListener(IConfigurationSource configuration)
    {
      // TODO: On windows, anyone can listen on port 80 at Temporary_Listen_Addresses
      // TODO: On Mono, not the case. So we probably need different logic for windows/mono
      // TODO: otherwise you need to be Admin to run these tests on Windows
      AppPathVDir = $"Temporary_Listen_Addresses/{Guid.NewGuid()}/";

      var isStarting = true;
      do
      {
        try
        {
          Port = Random.Next(2048, 4096);

          Host = new HttpListenerHost(configuration);
          Host.Initialize(new[] { $"http://+:{Port}/{AppPathVDir}" }, AppPathVDir, null);
          Host.StartListening();

          isStarting = false;
        }
        catch
        {
          // Ignore
        }
      } while (isStarting);
    }

    public string WebGet(string path)
    {
      using (var webClient = new WebClient())
      {
        return webClient.DownloadString($"http://localhost:{Port}/{AppPathVDir}{path}");
      }
    }

    public HttpListenerHost Host { get; }
    public string AppPathVDir { get; }
    public int Port { get; }
  }
}