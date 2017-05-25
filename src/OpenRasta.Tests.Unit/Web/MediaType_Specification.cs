
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenRasta.Web;
using Shouldly;

namespace MediaType_Specification
{
  [TestFixture]
  public class when_parsing_a_content_type
  {
    [Test]
    public void TheBaseTypeIsProcessedCorrectly()
    {
      MediaType content = new MediaType("application/xml");
      content.TopLevelMediaType.ShouldBe( "application");
      content.Subtype.ShouldBe( "xml");
      content.Quality.ShouldBe(1.0f);
    }

    [Test]
    public void invalid_values_are_ignored()
    {
      MediaType.Parse("application/xml,godknows,skiuroiu232u42iu4;;';")
        .ShouldHaveSingleItem()
        .ShouldBe(MediaType.Xml);
    }

    [Test]
    public void trailing_coma_is_ignored()
    {
      MediaType.Parse("application/xml,")
        .ShouldHaveSingleItem()
        .ShouldBe(MediaType.Xml);
    }
    [Test]
    public void no_valid_value_throws()
    {
      Should.Throw<FormatException>(() => MediaType.Parse("godknows,skiuroiu232u42iu4;;';"));
    }

    [Test, SetCulture("fr-FR")]
    public void parsing_a_quality_value_when_using_another_culture_still_parse_the_dot_value()
    {
      var mediaType = new MediaType("application/xml;q=0.3");
      mediaType.Quality.ShouldBe(0.3f);
    }
  }

  [TestFixture]
  public class when_ordering_content_types
  {
    [Test]
    public void AWildcardForBaseTypeIsLessSpecificThanANonWildcard()
    {
      var ct = MediaType.Parse("*/xml,text/xml").ToList();
      ct[0].MediaType.ShouldBe( "text/xml");
      ct[1].MediaType.ShouldBe( "*/xml");
    }

    [Test]
    public void AWildcardHasLowerPriorityThanAny()
    {
      var ct = MediaType.Parse("application/*, */*, application/xhtml+xml").ToList();
      ct[0].MediaType.ShouldBe( "application/xhtml+xml");
      ct[1].MediaType.ShouldBe( "application/*");
      ct[2].MediaType.ShouldBe( "*/*");
    }

    [Test]
    public void AWildcardIsConsideredAsHavingTheLowestPriority()
    {
      var ct = MediaType.Parse("*/*,text/plain;q=0.1").ToList();
      ct[0].MediaType.ShouldBe( "text/plain");
      ct[1].MediaType.ShouldBe( "*/*");
    }

    [Test]
    public void AWildcardSubTypeHasLowerPriorityThanASpecificSubType()
    {
      var ct = MediaType.Parse("application/*, application/xhtml+xml").ToList();
      ct[0].MediaType.ShouldBe( "application/xhtml+xml");
    }

    [Test]
    public void TheApplicationXmlContentTypeIsOrderedAfterAnyOtherContentTypeOfSamePriority()
    {
      var ct = MediaType.Parse("application/xml,text/plain,image/jpeg;q=0.7").ToList();
      ct[0].MediaType.ShouldBe( "text/plain");
      ct[1].MediaType.ShouldBe( "application/xml");
      ct[2].MediaType.ShouldBe( "image/jpeg");
    }

    [Test]
    public void TheApplicationXmlContentTypeIsOrderedAfterMoreSpecificTypes()
    {
      var ct = MediaType.Parse("application/xml, application/xhtml+xml").ToList();
      ct[0].MediaType.ShouldBe( "application/xhtml+xml");
    }

    [Test]
    public void TheOrderingWorksAsPerRFC2616_14_1()
    {
      var contentTypes = MediaType.Parse("text/plain; q=0.5, text/html, text/x-dvi; q=0.8, text/x-c").ToList();

      contentTypes[0].MediaType.ShouldBe( "text/x-c");
      contentTypes[1].MediaType.ShouldBe( "text/html");
      contentTypes[2].MediaType.ShouldBe( "text/x-dvi");
      contentTypes[3].MediaType.ShouldBe( "text/plain");
    }
  }
}