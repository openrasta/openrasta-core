using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Tests.Scenarios.HandlerReturns.LocationHeader
{
  public class absolute_uri : location_header<AbsUriHandler>
  {
    [Fact]
    public async Task header_is_correct()
    {
      var r = await Response;
      var rAsync = await ResponseAsync;
      
      r.StatusCode.ShouldBe(200);
      rAsync.StatusCode.ShouldBe(200);

      r.Headers["Location"].ShouldBe("http://localhost/absResource");
      rAsync.Headers["Location"].ShouldBe("http://localhost/absResource");
    }
  }
}