using System;
using OpenRasta.Configuration;
using OpenRasta.Plugins.Caching;
using Shouldly;
using Tests.Plugins.Caching.conditionals.if_modified_since;
using Tests.Plugins.Caching.contexts;
using Xunit;

namespace Tests.Plugins.Caching.last_modified
{
  public class already_set : caching
  {
    public already_set()
    {
      // 2.2.1
      // An origin server with a clock MUST NOT send a Last-Modified date that
      // is later than the server's time of message origination (Date).
      given_time(now);
      given_uses(_ => _.PipelineContributor<LastModifiedInPast>());
      given_resource<TestResource>(map => map.LastModified(_ => now));

      when_executing_request("/TestResource");
    }

    class LastModifiedInPast : HeaderSetter
    {
      public LastModifiedInPast() : base("last-modified",
        (ServerClock.UtcNow() - TimeSpan.FromSeconds(value: 10)).ToString("R"))
      {
      }
    }

    [Fact]
    public void last_modified_header_not_overridden()
    {
      DateTimeOffset.Parse(response.Headers["last-modified"])
        .ShouldNotBe(now.Value);
    }

    [Fact]
    public void request_successful()
    {
      response.StatusCode.ShouldBe(expected: 200);
    }
  }
}