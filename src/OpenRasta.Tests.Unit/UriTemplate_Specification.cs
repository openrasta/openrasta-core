using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using NUnit.Framework;
using OpenRasta;
using OpenRasta.Collections;
using OpenRasta.Collections.Specialized;
using OpenRasta.Tests.Unit.Infrastructure;
using Shouldly;

namespace UriTemplate_Specification
{
  public abstract class uritemplate_context : context
  {
    protected IEnumerable<Uri> BaseUris = new List<Uri> {new Uri("http://localhost")};

    protected IEnumerable<string> BindingUriByName(string template, object values)
    {
      var boundUris = BaseUris
        .Select(baseUri => new UriTemplate(template)
          .BindByName(baseUri, values.ToNameValueCollection()));
      return boundUris.Select(uri=>uri.ToString());
    }

    protected void GivenBaseUris(params string[] uris)
    {
      BaseUris = new List<Uri>(uris.Select(u => new Uri(u)));
    }
  }

  [TestFixture]
  public class when_accessing_path_segments : uritemplate_context
  {
    [Test]
    public void all_valid_variables_are_returned()
    {
      new UriTemplate("weather/{state}/{city}").PathSegmentVariableNames.ShouldBe((IEnumerable<string>) new[]
        {"STATE", "CITY"});
    }
  }

  [TestFixture]
  public class when_binding_urls_by_name : uritemplate_context
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
        .ShouldAllBe(item => item == "http://localhost/test?first=1&second=2");
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

  [TestFixture]
  public class when_binding_uris_by_name_in_a_vpath : uritemplate_context
  {
    public when_binding_uris_by_name_in_a_vpath()
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

  [TestFixture]
  public class when_matching_urls : uritemplate_context
  {
    UriTemplateMatch ThenTheMatch;

    void GivenAMatching(string template, string candidate)
    {
      GivenAMatching("http://localhost/", template, candidate);
    }

    void GivenAMatching(string baseUri, string template, string candidate)
    {
      ThenTheMatch = new UriTemplate(template).Match(baseUri.ToUri(), candidate.ToUri());
    }

    [Test]
    public void a_template_on_the_root_gets_a_match()
    {
      GivenAMatching("/", "http://localhost/");

      ThenTheMatch.ShouldNotBeNull();
    }

    [Test]
    public void matching_urls_with_different_host_names_returns_no_match()
    {
      var table = new UriTemplate("/temp");
      table.Match(new Uri("http://localhost"), new Uri("http://notlocalhost/temp")).ShouldBeNull();
    }

    [Test]
    public void matching_urls_with_different_casing_returns_match()
    {
      GivenAMatching("/weather", "http://localhost/Weather");
      ThenTheMatch.BaseUri.ShouldBe(BaseUris.First());
    }

    [Test]
    public void the_base_uri_is_the_one_provided_in_the_match()
    {
      GivenAMatching("/weather/{state}/{city}", "http://localhost/weather/Washington/Seattle");
      ThenTheMatch.BaseUri.ShouldBe(BaseUris.First());
    }

    [Test]
    public void the_match_has_the_correct_relative_path_segments()
    {
      GivenAMatching("weather/{state}/{city}", "http://localhost/weather/Washington/Seattle");
      ThenTheMatch.RelativePathSegments.ShouldBe((IEnumerable<string>) new[] {"weather", "Washington", "Seattle"});
    }

    [Test]
    public void the_match_has_the_correct_segment_variables()
    {
      GivenAMatching("/weather/{state}/{city}", "http://localhost/weather/Washington/Seattle");

      SpecializedCollectionExtensions.ToDictionary(ThenTheMatch.PathSegmentVariables)
        .ShouldBe(SpecializedCollectionExtensions.ToDictionary(new NameValueCollection().With("STATE", "Washington")
          .With("city", "Seattle")));
    }

    [Test]
    public void the_match_includes_dots()
    {
      GivenAMatching("/users/{username}", "http://localhost/users/sebastien.lambla");
      ThenTheMatch.PathSegmentVariables.ShouldBe(new NameValueCollection().With("username", "sebastien.lambla"));
    }
    
    
    [Test(Description="See https://github.com/openrasta/openrasta-core/issues/180")]
    public void qs_match_includes_braces()
    {
      GivenAMatching("parties?cvrcpr={cvrcpr}&cvr_p={cvr_p}", "http://localhost/parties?cvrcpr={cvrcpr}&cvr_p={cvr_p}");
      ThenTheMatch.QueryStringVariables["CVRCPR"].ShouldBe("{cvrcpr}");
    }
    
    [Test(Description="See https://github.com/openrasta/openrasta-core/issues/180")]
    public void seg_match_includes_braces()
    {
      GivenAMatching("parties/{cvrcpr}", "http://localhost/parties/{cvrcpr}");
      ThenTheMatch.PathSegmentVariables["CVRCPR"].ShouldBe("{cvrcpr}");
    }

    [Test]
    public void the_template_matches_when_in_a_virtual_directory()
    {
      GivenAMatching("http://localhost/vdir/", "/test/", "http://localhost/vdir/test/");
      ThenTheMatch.ShouldNotBeNull();
    }

    [Test]
    public void the_template_matches_when_in_a_virtual_directory_without_trailing_slash()
    {
      GivenAMatching("http://localhost/vdir", "/test/", "http://localhost/vdir/test/");
      ThenTheMatch.ShouldNotBeNull();
    }

    [Test]
    public void there_is_no_match_when_a_segment_doesnt_match()
    {
      GivenAMatching("/weather/{state}/{city}", "http://localhost/temperature/Washington/Seattle");

      ThenTheMatch.ShouldBeNull();
    }

    [Test]
    public void there_is_no_match_with_different_segment_counts()
    {
      GivenAMatching("/weather/{state}/{city}", "http://localhost/nowt");

      ThenTheMatch.ShouldBeNull();
    }
  }


  [TestFixture]
  public class when_using_fragment_identifiers : uritemplate_context
  {
    [Test]
    public void BindingReturnsGeneratedUri()
    {
      var baseUri = new Uri("http://localhost");
      var variableValues = new NameValueCollection().With("state", "washington").With("City",
        "seattle");

      var bindByName = new UriTemplate("weather#{state}/{city}/").BindByName(baseUri, variableValues).ToString();
      bindByName
        .ShouldBe("http://localhost/weather#washington/seattle/");

      bindByName = new UriTemplate("weather/#{state}/{city}/").BindByName(baseUri, variableValues).ToString();
      bindByName
        .ShouldBe("http://localhost/weather/#washington/seattle/");

      bindByName = new UriTemplate("weather/#/{state}/{city}/").BindByName(baseUri, variableValues).ToString();
      bindByName
        .ShouldBe("http://localhost/weather/#/washington/seattle/");
    }
  }

  [TestFixture]
  public class when_binding_by_name : uritemplate_context
  {
    [Test]
    public void a_wildcard_is_not_generated()
    {
      var baseUri = new Uri("http://localhost");
      NameValueCollection variableValues = new NameValueCollection().With("state", "washington").With("CitY",
        "seattle");

      new UriTemplate("weather/{state}/{city}/*").BindByName(baseUri, variableValues)
        .ShouldBe("http://localhost/weather/washington/seattle/".ToUri());
    }

    [Test]
    public void the_variable_names_are_not_case_sensitive()
    {
      var baseUri = new Uri("http://localhost");
      NameValueCollection variableValues = new NameValueCollection().With("StAte", "washington").With("CitY",
        "seattle");

      new UriTemplate("weather/{state}/{city}/").BindByName(baseUri, variableValues)
        .ShouldBe("http://localhost/weather/washington/seattle/".ToUri());
    }

    [Test]
    public void the_variables_are_replaced_in_the_generated_uri()
    {
      NameValueCollection variableValues = new NameValueCollection().With("state", "washington").With("city",
        "seattle");

      new UriTemplate("weather/{state}/{city}/").BindByName("http://localhost".ToUri(), variableValues)
        .ShouldBe(new Uri("http://localhost/weather/washington/seattle/"));
    }
  }

  public class when_matching_querystrings : uritemplate_context
  {
    [Test]
    public void a_query_parameter_with_no_variable_is_ignored()
    {
      var template = new UriTemplate("/test?query=3");
      template.QueryStringVariableNames.Count.ShouldBe(0);
    }

    [Test]
    public void a_url_matching_result_in_the_query_value_variable_being_set()
    {
      var table = new UriTemplate("/test?query={queryValue}");
      UriTemplateMatch match =
        table.Match(new Uri("http://localhost"), new Uri("http://localhost/test?query=search"));

      match.ShouldNotBeNull();

      match.QueryStringVariables["queryValue"].ShouldBe("search");
    }

    [Test]
    public void a_url_not_matching_a_literal_query_string_will_not_match()
    {
      var table = new UriTemplate("/test?query=literal");
      UriTemplateMatch match = table.Match(new Uri("http://localhost"),
        new Uri("http://localhost/test?query=notliteral"));
      match.ShouldBeNull();
    }

    [Test]
    public void multiple_query_parameters_are_processed()
    {
      var template = new UriTemplate("/test?query1={test}&query2={test2}");
      template.QueryStringVariableNames.Contains("test").ShouldBeTrue();
      template.QueryStringVariableNames.Contains("test2").ShouldBeTrue();
    }

    [Test]
    public void query_string_segments_are_filled()
    {
      var template = new UriTemplate("/test?query1={test}&query2={test2}&query3=val");
      var query1 = template.QueryString.ElementAt(0);
      query1.Key.ShouldBe("query1");
      query1.Value.ShouldBe("test");
      query1.Type.ShouldBe(UriTemplate.SegmentType.Variable);

      var query2 = template.QueryString.ElementAt(1);
      query2.Key.ShouldBe("query2");
      query2.Value.ShouldBe("test2");
      query2.Type.ShouldBe(UriTemplate.SegmentType.Variable);

      var query3 = template.QueryString.ElementAt(2);
      query3.Key.ShouldBe("query3");
      query3.Value.ShouldBe("val");
      query3.Type.ShouldBe(UriTemplate.SegmentType.Literal);

      template.QueryStringVariableNames.Contains("test").ShouldBeTrue();
      template.QueryStringVariableNames.Contains("test2").ShouldBeTrue();
    }

    [Test]
    public void a_url_matching_multiple_query_parameters_should_match()
    {
      var template = new UriTemplate("/test?query1={test}&query2={test2}");
      var match = template.Match(new Uri("http://localhost"),
        new Uri("http://localhost/test?query1=test1&query2=test2"));
      match.ShouldNotBeNull();
    }

    [Test]
    public void a_parameter_different_by_last_letter_to_query_parameters_should_not_match()
    {
      var template = new UriTemplate("/test?query1={test}&query2={test2}");
      var match = template.Match(new Uri("http://localhost"),
        new Uri("http://localhost/test?query1=test1&query3=test2"));
      match.ShouldNotBeNull();
      match.PathSegmentVariables.Count.ShouldBe(0);
      match.QueryStringVariables.Count.ShouldBe(1);
      match.QueryParameters.Count.ShouldBe(2);
    }

    [Test]
    public void more_than_two_query_parameters_with_similar_names_are_processed()
    {
      var template = new UriTemplate("/test?query1={test}&query2={test2}&query3={test3}");
      template.QueryStringVariableNames.Contains("test").ShouldBeTrue();
      template.QueryStringVariableNames.Contains("test2").ShouldBeTrue();
      template.QueryStringVariableNames.Contains("test3").ShouldBeTrue();
    }

    [Test]
    public void a_url_matching_three_query_string_parameters_will_match()
    {
      var table = new UriTemplate("/test?q={searchTerm}&p={pageNumber}&s={pageSize}");
      UriTemplateMatch match =
        table.Match(new Uri("http://localhost"), new Uri("http://localhost/test?q=&p=1&s=10"));
      match.ShouldNotBeNull();
      match.QueryStringVariables["searchTerm"].ShouldBe(string.Empty);
      match.QueryStringVariables["pageNumber"].ShouldBe("1");
      match.QueryStringVariables["pageSize"].ShouldBe("10");
    }

    [Test]
    public void a_url_with_extra_query_string_parameters_will_match()
    {
      var template = new UriTemplate("/test?q={searchTerm}&p={pageNumber}&s={pageSize}");
      UriTemplateMatch match = template.Match(new Uri("http://localhost/"),
        new Uri("http://localhost/test?q=test&p=1&s=10&contentType=json"));
      match.ShouldNotBeNull();
    }


    [Test]
    public void the_query_parameters_are_exposed()
    {
      var table = new UriTemplate("/test?query={queryValue}");
      table.QueryStringVariableNames.Contains("queryValue").ShouldBeTrue();
    }

    [Test]
    public void the_query_parameters_should_be_case_insensitive()
    {
      var template = new UriTemplate("/test?page={page}");

      var match = template.Match(new Uri("http://localhost"), new Uri("http://localhost/test?pAgE=2"));

      match.ShouldNotBeNull();
      match.QueryStringVariables.Count.ShouldBe(1);
      match.QueryStringVariables["PAGE"].ShouldBe("2");
    }

    [Test]
    public void the_template_matches_when_query_strings_are_not_present()
    {
      var template = new UriTemplate("/temperature?unit={unit}");
      var match = template.Match(new Uri("http://localhost"), new Uri("http://localhost/temperature"));

      match.ShouldNotBeNull();
      match.PathSegmentVariables.Count.ShouldBe(0);
      match.QueryParameters.Count.ShouldBe(1);
    }

    [Test]
    public void the_template_matches_when_query_strings_are_present()
    {
      var template = new UriTemplate("/temperature?unit={unit}");
      var match = template.Match(new Uri("http://localhost"), new Uri("http://localhost/temperature"));

      match.ShouldNotBeNull();
      match.PathSegmentVariables.Count.ShouldBe(0);
      match.QueryParameters.Count.ShouldBe(1);
    }
  }

  [TestFixture]
  public class when_comparing_templates : uritemplate_context
  {
    bool TheResult;

    void GivenTwoTemplates(string template1, string template2)
    {
      TheResult = new UriTemplate(template1).IsEquivalentTo(new UriTemplate(template2));
    }

    [Test]
    public void a_template_isnt_equivalent_to_a_null_reference()
    {
      TheResult = new UriTemplate("weather/{state}/{city}").IsEquivalentTo(null);

      TheResult.ShouldBeFalse();
    }

    [Test]
    public void AWildcardMappingDoesntMatchANonWildcard()
    {
      GivenTwoTemplates(
        "weather/{state}/*",
        "weather/{state}/something");

      TheResult.ShouldBeFalse();
    }

    [Test]
    public void different_number_of_segments_make_two_templates_not_equivalent()
    {
      GivenTwoTemplates(
        "weather/{state}/{city}",
        "weather/{country}/");

      TheResult.ShouldBeFalse();
    }

    [Test]
    public void templates_with_different_query_strings_are_not_equivalent()
    {
      GivenTwoTemplates(
        "weather/{state}/{city}?forecast={day}&temp=1",
        "weather/{state}/{city}?forecast={day}");
      TheResult.ShouldBeFalse();
    }

    [Test]
    public void the_literal_path_segments_must_match()
    {
      GivenTwoTemplates(
        "weather/{state}/test",
        "weather/{state}/test2");
      TheResult.ShouldBeFalse();
    }

    [Test]
    public void the_preceding_slash_character_is_ignored()
    {
      GivenTwoTemplates(
        "weather/{state}/{city}?forecast={day}",
        "/weather/{country}/{village}?forecast={type}");
      TheResult.ShouldBeTrue();
    }

    [Test]
    public void the_trailing_slash_character_is_ignored()
    {
      GivenTwoTemplates(
        "weather/{state}/{city}",
        "weather/{country}/{village}/");
      TheResult.ShouldBeTrue();
    }

    [Test]
    public void the_trailing_slash_character_is_ignored_after_a_wildcard()
    {
      GivenTwoTemplates(
        "weather/{state}/*",
        "weather/{state}/*/");
      TheResult.ShouldBeFalse();
    }
  }
}