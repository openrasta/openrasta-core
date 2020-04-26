using NUnit.Framework;
using Shouldly;

namespace OpenRasta.Tests.Unit.UriTemplates
{
  [TestFixture]
  public class bind_by_name_vpath : uritemplate_context
  {
    public bind_by_name_vpath()
    {
      GivenBaseUris("http://localhost/foo", "http://localhost/foo/");
    }

    [Test]
    public void a_query_string_is_appended_successfully()
    {
      BindingUriByName("?query={value}", new {value = "myQuery"})
        .ShouldAllBe(item => item == "http://localhost/foo/?query=myQuery");
    }

    [Test]
    public void a_segment_is_appended_successfully()
    {
      BindingUriByName("/{value}", new {value = "myQuery"}).ShouldAllBe(item => item == "http://localhost/foo/myQuery");
    }

    [Test]
    public void an_unprefixed_segment_is_appended_successfully()
    {
      BindingUriByName("{value}", new {value = "myQuery"}).ShouldAllBe(item => item == "http://localhost/foo/myQuery");
    }
  }
}