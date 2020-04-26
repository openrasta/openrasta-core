using System;
using System.Collections.Specialized;
using System.Linq;
using NUnit.Framework;
using OpenRasta.Collections;
using OpenRasta.Collections.Specialized;
using Shouldly;
using UriTemplateTable_Specification;

namespace OpenRasta.Tests.Unit.UriTemplates
{
  [TestFixture]
  public class matching_segments : uritemplate_context
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

    [Test, Ignore("Temporarily not supporting this till new URI templating is done")]
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
      ThenTheMatch.RelativePathSegments.ShouldBe(new[] {"weather", "Washington", "Seattle"});
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
    
    
    [GitHubIssue(180)]
    public void qs_match_includes_braces()
    {
      GivenAMatching("parties?cvrcpr={cvrcpr}&cvr_p={cvr_p}", "http://localhost/parties?cvrcpr={cvrcpr}&cvr_p={cvr_p}");
      ThenTheMatch.QueryStringVariables["CVRCPR"].ShouldBe("{cvrcpr}");
    }
    
    [GitHubIssue(180)]
    public void seg_match_includes_braces()
    {
      GivenAMatching("parties/{cvrcpr}", "http://localhost/parties/{cvrcpr}");
      ThenTheMatch.PathSegmentVariables["CVRCPR"].ShouldBe("{cvrcpr}");
    }

    [Test]
    public void qs_value_with_equal_matched()
    {
      GivenAMatching("/path/?q={where}&order={order}", "http://localhost/path/?q=value=123&order=law");
      ThenTheMatch.QueryStringVariables["where"].ShouldBe("value=123");
      ThenTheMatch.QueryStringVariables["order"].ShouldBe("law");
    }
    [Test]
    public void qs_value_with_encoded_equal_matched()
    {
      GivenAMatching("/path/?q={where}&order={order}", "http://localhost/path/?q=value%3D123&order=law");
      ThenTheMatch.QueryStringVariables["where"].ShouldBe("value=123");
      ThenTheMatch.QueryStringVariables["order"].ShouldBe("law");
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
}