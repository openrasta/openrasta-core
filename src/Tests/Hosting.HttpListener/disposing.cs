using OpenRasta.Hosting.HttpListener;
using Shouldly;
using Xunit;

namespace Tests.Hosting.HttpListener
{
  public class disposing
  {
    [Fact]
    public void disposes_cleanly()
    {
      Should.NotThrow(() => new HttpListenerHost().Close());
    }
  }
}