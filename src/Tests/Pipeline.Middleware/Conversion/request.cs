using System.Linq;
using OpenRasta.Pipeline;
using Shouldly;
using Tests.Pipeline.Middleware.Examples;
using Tests.Pipeline.Middleware.Infrastructrure;
using Xunit;

namespace Tests.Pipeline.Middleware.Conversion
{
  public class request
  {
    [Fact]
    public void convers_to_pre_exec_contrib()
    {
      var calls = new ContributorCall[]
      {
        new ContributorCall(new DoNothingContributor(), OpenRasta.Pipeline.Middleware.IdentitySingleTap, "before"),
        new ContributorCall(new UriMatchingContributor(), OpenRasta.Pipeline.Middleware.IdentitySingleTap, "uri"),
        new ContributorCall(new DoNothingContributor(), OpenRasta.Pipeline.Middleware.IdentitySingleTap, "after")
      };
      var middlewareChain = calls.ToMiddleware().ToArray();
      middlewareChain[0].ShouldBeOfType<PreExecuteMiddleware>();
      middlewareChain[1].ShouldBeOfType<RequestMiddleware>();
      middlewareChain[2].ShouldBeOfType<RequestMiddleware>();
    }
  }
}