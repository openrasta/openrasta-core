using NUnit.Framework;
using Shouldly;

namespace OpenRasta.Tests.Unit.UriTemplates
{
  [TestFixture]
  public class bind_by_name_qs : uritemplate_context
  {
    [Test]
    public void the_values_in_the_query_string_are_injected()
    {
      BindingUriByName("/test?query={value}", new {value = "myQuery"})
        .ShouldAllBe(item => item == "http://localhost/test?query=myQuery");
    }

    [Test]
    public void a_query_string_with_separator_is_injected()
    {
      BindingUriByName("/test?first={first}&?second={second}", new {first = "1", second = "2"})
        .ShouldAllBe(item => item == "http://localhost/test?first=1&?second=2");
    }

    [Test]
    public void qs_value_is_encoded()
    {
      BindingUriByName("/?value={value}", new {value = "&query"})
        .ShouldAllBe(item => item == "http://localhost/?value=%25query");
    }

    [Test]
    public void qs_previous_separators_not_encoded()
    {
      BindingUriByName("/?value={value}", new {value = "/?query"})
        .ShouldAllBe(item => item == "http://localhost/?value=/?query");
    }

    [Test]
    public void segment_value_is_encoded()
    {
      BindingUriByName("/{value}", new {value = "?query"}).ShouldAllBe(item => item == "http://localhost/%3Fquery");
    }
    
    [Test(Description="See https://github.com/openrasta/openrasta-core/issues/180")]
    public void braces_are_present_in_segments()
    {
      BindingUriByName("/{value}", new {value = "{query}"}).ShouldAllBe(item => item == "http://localhost/{query}");
    }
    
    [Test(Description="See https://github.com/openrasta/openrasta-core/issues/180")]
    public void braces_are_present_in_qs()
    {
      BindingUriByName("/?query={value}", new {value = "{query}"}).ShouldAllBe(item => item == "http://localhost/?query={query}");
    }
  }
}