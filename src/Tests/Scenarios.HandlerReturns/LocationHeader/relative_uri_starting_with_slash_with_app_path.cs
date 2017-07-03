using System.Threading.Tasks;
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
    public async Task location_header_abs_path_is_relative_to_app_base()
    {
      var r = Response;
      var rAsync = ResponseAsync;
      
      r.StatusCode.ShouldBe(200);
      rAsync.StatusCode.ShouldBe(200);

      r.Headers["Location"].ShouldBe("http://localhost/myApp/absPathResource");
      rAsync.Headers["Location"].ShouldBe("http://localhost/myApp/absPathResource");
    }
  }
}