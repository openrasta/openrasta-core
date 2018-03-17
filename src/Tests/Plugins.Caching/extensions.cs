using System;
using OpenRasta.Plugins.Caching.Configuration;
using Shouldly;
using Xunit;

namespace Tests.Plugins.Caching
{
  public class extensions
  {
    [Fact]
    public void after_date()
    {
      var now = DateTimeOffset.Now;
      now.ShouldBeGreaterThan(now - 2.Hours());
    }

    [Fact]
    public void before_date()
    {
      var now = DateTimeOffset.Now;
      now.ShouldBeLessThan(now + 2.Hours());
    }
  }
}