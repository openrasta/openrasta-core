using System.Linq;
using OpenRasta.Pipeline;
using Shouldly;
using Tests.Pipeline.Middleware.Infrastructrure;
using Xunit;

namespace Tests.Pipeline.Middleware.Conversion
{
  public class pre_execute
  {
    [Fact]
    public void convers_to_pre_exec_contrib()
    {
      var calls = new[]
      {
        new ContributorCall(new DoNothingContributor(), OpenRasta.Pipeline.Middleware.IdentitySingleTap, "doNothing")
      };
      var middlewareChain = calls.ToMiddleware();
      middlewareChain.First().ShouldBeOfType<PreExecuteMiddleware>();
    }
  }
}