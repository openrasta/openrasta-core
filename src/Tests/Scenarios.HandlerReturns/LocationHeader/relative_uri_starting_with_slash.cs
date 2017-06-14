using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerReturns.LocationHeader
{
  public class relative_uri_starting_with_slash : location_header<RelUriAbsPath>
  {
    [Fact]
    public void location_header_is_relative_uri_abs_path()
    {
      Response.StatusCode.ShouldBe(200);
      ResponseAsync.StatusCode.ShouldBe(200);

      Response.Headers["Location"].ShouldBe("http://localhost/absPathResource");
      ResponseAsync.Headers["Location"].ShouldBe("http://localhost/absPathResource");
    }
  }
}