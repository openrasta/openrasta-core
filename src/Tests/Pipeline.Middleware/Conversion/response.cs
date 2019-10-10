using System.Linq;
using OpenRasta.Concordia;
using OpenRasta.Pipeline;
using OpenRasta.Pipeline.Contributors;
using Shouldly;
using Tests.Pipeline.Middleware.Examples;
using Xunit;

namespace Tests.Pipeline.Middleware.Conversion
{
  public class response
  {
    [Fact]
    public void convert_to_post_execute_middleware()
    {
      var calls = new[]
      {
        new ContributorCall(new DoNothingContributor(), OpenRasta.Pipeline.Middleware.IdentitySingleTap, "before"),
        new ContributorCall(new UriMatchingContributor(), OpenRasta.Pipeline.Middleware.IdentitySingleTap, "uri"),
        new ContributorCall(new OperationResultContributor(), OpenRasta.Pipeline.Middleware.IdentitySingleTap, "result"),
        new ContributorCall(new RequestResponseDisposer(), OpenRasta.Pipeline.Middleware.IdentitySingleTap,"end"),
        new ContributorCall(new DoNothingContributor(), OpenRasta.Pipeline.Middleware.IdentitySingleTap, "response"),

      };
      var middlewareChain = calls.ToMiddleware(new StartupProperties()).ToArray();
      
      middlewareChain[0].ShouldBeOfType<PreExecuteMiddleware>();
      middlewareChain[1].ShouldBeOfType<RequestMiddleware>();
      middlewareChain[2].ShouldBeOfType<OpenRasta.Pipeline.ResponseRetryMiddleware>();
      middlewareChain[3].ShouldBeOfType<ResponseMiddleware>();
      middlewareChain[4].ShouldBeOfType<PostExecuteMiddleware>();
      middlewareChain[5].ShouldBeOfType<PostExecuteMiddleware>();
    }
  }
}