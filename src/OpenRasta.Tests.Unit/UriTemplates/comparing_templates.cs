using NUnit.Framework;
using Shouldly;

namespace OpenRasta.Tests.Unit.UriTemplates
{
  [TestFixture]
  public class comparing_templates : uritemplate_context
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
      GivenTwoTemplates("weather/{state}/*", "weather/{state}/something");

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