using System;
using OpenRasta.Configuration;
using OpenRasta.Plugins.Caching;
using Shouldly;
using Tests.Plugins.Caching.contexts;
using Xunit;

namespace Tests.Plugins.Caching.conditionals.if_modified_since
{
  public class last_modified_in_future : caching
  {
    public last_modified_in_future()
    {
      // 2.2.1
      // An origin server with a clock MUST NOT send a Last-Modified date that
      // is later than the server's time of message origination (Date).
      given_current_time(now);
      given_uses(_ => _.PipelineContributor<LastModifiedInFuture>());
      given_resource<TestResource>();
      given_request_header("if-modified-since", now);

      when_executing_request("/TestResource");
    }

    class LastModifiedInFuture : HeaderSetter
    {
      public LastModifiedInFuture() : base("last-modified",
        (ServerClock.UtcNow() + TimeSpan.FromSeconds(value: 10)).ToString("R"))
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
    public void not_modified()
    {
      response.StatusCode.ShouldBe(expected: 304);
    }
  }
}