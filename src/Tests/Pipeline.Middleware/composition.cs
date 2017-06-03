using System.Collections.Generic;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Pipeline;
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
        new DelegateBeforeNextMiddleware(() => ordered.Add("first")),
        new DelegateBeforeNextMiddleware(() => ordered.Add("second")),
      };
      contribs.Compose().Invoke(new InMemoryCommunicationContext());
      ordered.ShouldBe(new[]{"first", "second"});
    }
  }
}