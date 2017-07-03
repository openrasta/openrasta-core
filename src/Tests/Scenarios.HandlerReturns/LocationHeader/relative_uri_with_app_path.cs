using System.Threading.Tasks;
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
    public async Task location_is_absolute()
    {
      var r = Response;
      var rAsync = ResponseAsync;
      
      r.StatusCode.ShouldBe(200);
      rAsync.StatusCode.ShouldBe(200);

      r.Headers["Location"].ShouldBe("http://localhost/myApp/resource/relPathResource");
      rAsync.Headers["Location"].ShouldBe("http://localhost/myApp/resource/async/relPathResource");
    }
  }
}