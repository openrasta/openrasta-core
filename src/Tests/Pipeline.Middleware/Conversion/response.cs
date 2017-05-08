using System.Linq;
using OpenRasta.Pipeline;
using Shouldly;
using Tests.Pipeline.Middleware.Examples;
using Tests.Pipeline.Middleware.Infrastructrure;
using Xunit;

namespace Tests.Pipeline.Middleware.Conversion
{
  public class response
  {
    [Fact]
    public void convers_to_pre_exec_contrib()
    {
      var calls = new ContributorCall[]
      {
        new ContributorCall(new DoNothingContributor(), OpenRasta.Pipeline.Middleware.IdentitySingleTap, "before"),
        new ContributorCall(new UriMatchingContributor(), OpenRasta.Pipeline.Middleware.IdentitySingleTap, "uri"),
        new ContributorCall(new DoNothingContributor(), OpenRasta.Pipeline.Middleware.IdentitySingleTap, "request"),
        new ContributorCall(new OperationResultContributor(), OpenRasta.Pipeline.Middleware.IdentitySingleTap, "result"),
        new ContributorCall(new DoNothingContributor(), OpenRasta.Pipeline.Middleware.IdentitySingleTap, "request")
      };
      var middlewareChain = calls.ToMiddleware().ToArray();
      middlewareChain[0].ShouldBeOfType<PreExecuteMiddleware>();
      middlewareChain[1].ShouldBeOfType<PreExecuteMiddleware>();
      middlewareChain[2].ShouldBeOfType<RequestMiddleware>();
      middlewareChain[3].ShouldBeOfType<RequestMiddleware>();
      middlewareChain[4].ShouldBeOfType<ResponseMiddleware>();
    }
  }
}