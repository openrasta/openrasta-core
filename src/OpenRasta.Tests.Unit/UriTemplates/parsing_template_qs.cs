using System;
using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace OpenRasta.Tests.Unit.UriTemplates
{
  public class parsing_template_qs : uritemplate_context
  {
    [Test]
    public void a_query_parameter_with_no_variable_is_ignored()
    {
      var template = new UriTemplate("/test?query=3");
      template.QueryStringVariableNames.Count.ShouldBe(0);
    }

    [Test]
    public void multiple_query_parameters_are_processed()
    {
      var template = new UriTemplate("/test?query1={test}&query2={test2}");
      template.QueryStringVariableNames.ShouldContain("test");
      template.QueryStringVariableNames.Contains("test2").ShouldBeTrue();
    }

    [Test]
    public void a_url_matching_result_in_the_query_value_variable_being_set()
    {
      var table = new UriTemplate("/test?query={queryValue}");
      var match =
        table.Match(new Uri("http://localhost"), new Uri("http://localhost/test?query=search"));

      match.ShouldNotBeNull();

      match.QueryStringVariables["queryValue"].ShouldBe("search");
    }

    [Test]
    public void a_url_not_matching_a_literal_query_string_will_not_match()
    {
      var table = new UriTemplate("/test?query=literal");
      var match = table.Match(new Uri("http://localhost"),
        new Uri("http://localhost/test?query=notliteral"));
      match.ShouldBeNull();
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
      var match =
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
      var match = template.Match(new Uri("http://localhost/"),
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
}