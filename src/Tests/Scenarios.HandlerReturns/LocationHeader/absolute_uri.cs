using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerReturns.LocationHeader
{
  public class absolute_uri : location_header<AbsUriHandler>
  {
    [Fact]
    public void header_is_correct()
    {
      Response.StatusCode.ShouldBe(200);
      ResponseAsync.StatusCode.ShouldBe(200);

      Response.Headers["Location"].ShouldBe("http://localhost/absResource");
      ResponseAsync.Headers["Location"].ShouldBe("http://localhost/absResource");
    }
  }
}