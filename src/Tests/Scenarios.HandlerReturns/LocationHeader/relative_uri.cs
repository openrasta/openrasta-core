using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerReturns.LocationHeader
{
  public class relative_uri : location_header<RelUriRelPath>
  {
    [Fact]
    public  void location_is_absolute()
    {
      var r =  Response;
      var rAsync =  ResponseAsync;
      r.StatusCode.ShouldBe(200);
      rAsync.StatusCode.ShouldBe(200);

      r.Headers["Location"].ShouldBe("http://localhost/resource/relPathResource");
      rAsync.Headers["Location"].ShouldBe("http://localhost/resource/async/relPathResource");
    }
  }
}