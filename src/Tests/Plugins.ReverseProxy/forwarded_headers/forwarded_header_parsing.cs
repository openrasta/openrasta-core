using System.Linq;
using OpenRasta.Plugins.ReverseProxy;
using Shouldly;
using Xunit;

namespace Tests.Plugins.ReverseProxy.forwarded_headers
{
  public class forwarded_header_parsing
  {
    [Fact]
    public void one_key_with_token_value_is_parsed()
    {
      var header = ForwardedHeader.Parse("key=value")
          .ShouldHaveSingleItem();
      header["key"].ShouldBe("value");
    }
        
    [Fact]
    public void multiple_pairs_with_token_value()
    {
      var header = ForwardedHeader.Parse("key=value;another=anothervalue")
          .ShouldHaveSingleItem();
      header["key"].ShouldBe("value");
      header["another"].ShouldBe("anothervalue");
    }
    [Fact]
    public void key_with_quoted_string_value()
    {
      var header = ForwardedHeader.Parse("key=\"value with space\"")
          .ShouldHaveSingleItem();
      header["key"].ShouldBe("value with space");
    }
    [Fact]
    public void key_with_empty_quoted_string()
    {
      var header = ForwardedHeader.Parse("key=\"\"")
          .ShouldHaveSingleItem();
      header["key"].ShouldBe("");
    }
    [Fact]
    public void multiple_key_value()
    {
      var header = ForwardedHeader.Parse("key=one,key=two");
          
      header.ElementAt(0)["key"].ShouldBe("one");
      header.ElementAt(1)["key"].ShouldBe("two");
    }
  }
}