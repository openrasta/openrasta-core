using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerReturns.LocationHeader
{
  public class relative_uri_starting_with_slash : location_header<RelUriAbsPath>
  {
    [Fact]
    public async Task location_header_is_relative_uri_abs_path()
    {
      var r = await Response;
      var rAsync = await ResponseAsync;
      
      r.StatusCode.ShouldBe(200);
      rAsync.StatusCode.ShouldBe(200);

      r.Headers["Location"].ShouldBe("http://localhost/absPathResource");
      rAsync.Headers["Location"].ShouldBe("http://localhost/absPathResource");
    }
  }
}