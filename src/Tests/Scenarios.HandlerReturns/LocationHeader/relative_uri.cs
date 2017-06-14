using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerReturns.LocationHeader
{
  public class relative_uri : location_header<RelUriRelPath>
  {
    [Fact]
    public void location_is_absolute()
    {
      Response.StatusCode.ShouldBe(200);
      ResponseAsync.StatusCode.ShouldBe(200);

      Response.Headers["Location"].ShouldBe("http://localhost/resource/relPathResource");
      ResponseAsync.Headers["Location"].ShouldBe("http://localhost/resource/async/relPathResource");
    }
  }
}