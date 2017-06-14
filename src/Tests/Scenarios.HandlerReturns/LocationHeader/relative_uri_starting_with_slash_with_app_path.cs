using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerReturns.LocationHeader
{
  public class relative_uri_starting_with_slash_with_app_path : location_header<RelUriAbsPath>
  {
    public relative_uri_starting_with_slash_with_app_path() : base("/myApp/")
    {
    }

    [Fact]
    public void location_header_abs_path_is_relative_to_app_base()
    {
      Response.StatusCode.ShouldBe(200);
      ResponseAsync.StatusCode.ShouldBe(200);

      Response.Headers["Location"].ShouldBe("http://localhost/myApp/absPathResource");
      ResponseAsync.Headers["Location"].ShouldBe("http://localhost/myApp/absPathResource");
    }
  }
}