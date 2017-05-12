using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Pipeline;
using OpenRasta.Web;
using Shouldly;
using Xunit;

namespace Tests.Pipeline.Middleware
{
  public class composition
  {
    [Fact]
    public void contributors_execute_in_order()
    {
      var ordered = new List<string>();
      var contribs = new[]
      {
        new DelegateMiddleware(() => ordered.Add("first")),
        new DelegateMiddleware(() => ordered.Add("second")),
      };
      contribs.Compose().Invoke(new InMemoryCommunicationContext());
      ordered.ShouldBe(new[]{"first", "second"});
    }
  }

  public class DelegateMiddleware : IPipelineMiddleware, IPipelineMiddlewareFactory
  {
    readonly Action _beforeNext;

    public DelegateMiddleware(Action beforeNext)
    {
      _beforeNext = beforeNext;
    }

    public Task Invoke(ICommunicationContext env)
    {
      _beforeNext();
      return Next.Invoke(env);
    }

    public IPipelineMiddleware Compose(IPipelineMiddleware next)
    {
      this.Next = next;
      return this;
    }

    public IPipelineMiddleware Next { get; set; }
  }
}