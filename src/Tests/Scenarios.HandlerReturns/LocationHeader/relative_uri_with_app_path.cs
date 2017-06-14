using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerReturns.LocationHeader
{
  public class relative_uri_with_app_path : location_header<RelUriRelPath>
  {
    public relative_uri_with_app_path() : base("/myApp/")
    {
      
    }
    [Fact]
    public void location_is_absolute()
    {
      Response.StatusCode.ShouldBe(200);
      ResponseAsync.StatusCode.ShouldBe(200);

      Response.Headers["Location"].ShouldBe("http://localhost/myApp/resource/relPathResource");
      ResponseAsync.Headers["Location"].ShouldBe("http://localhost/myApp/resource/async/relPathResource");
    }
  }
}