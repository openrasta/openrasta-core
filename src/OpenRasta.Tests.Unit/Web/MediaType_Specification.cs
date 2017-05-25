
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
      ShouldBeTestExtensions.ShouldBe(content.TopLevelMediaType, "application");
      ShouldBeTestExtensions.ShouldBe(content.Subtype, "xml");
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
      ShouldBeTestExtensions.ShouldBe(ct[0].MediaType, "text/xml");
      ShouldBeTestExtensions.ShouldBe(ct[1].MediaType, "*/xml");
    }

    [Test]
    public void AWildcardHasLowerPriorityThanAny()
    {
      var ct = MediaType.Parse("application/*, */*, application/xhtml+xml").ToList();
      ShouldBeTestExtensions.ShouldBe(ct[0].MediaType, "application/xhtml+xml");
      ShouldBeTestExtensions.ShouldBe(ct[1].MediaType, "application/*");
      ShouldBeTestExtensions.ShouldBe(ct[2].MediaType, "*/*");
    }

    [Test]
    public void AWildcardIsConsideredAsHavingTheLowestPriority()
    {
      var ct = MediaType.Parse("*/*,text/plain;q=0.1").ToList();
      ShouldBeTestExtensions.ShouldBe(ct[0].MediaType, "text/plain");
      ShouldBeTestExtensions.ShouldBe(ct[1].MediaType, "*/*");
    }

    [Test]
    public void AWildcardSubTypeHasLowerPriorityThanASpecificSubType()
    {
      var ct = MediaType.Parse("application/*, application/xhtml+xml").ToList();
      ShouldBeTestExtensions.ShouldBe(ct[0].MediaType, "application/xhtml+xml");
    }

    [Test]
    public void TheApplicationXmlContentTypeIsOrderedAfterAnyOtherContentTypeOfSamePriority()
    {
      var ct = MediaType.Parse("application/xml,text/plain,image/jpeg;q=0.7").ToList();
      ShouldBeTestExtensions.ShouldBe(ct[0].MediaType, "text/plain");
      ShouldBeTestExtensions.ShouldBe(ct[1].MediaType, "application/xml");
      ShouldBeTestExtensions.ShouldBe(ct[2].MediaType, "image/jpeg");
    }

    [Test]
    public void TheApplicationXmlContentTypeIsOrderedAfterMoreSpecificTypes()
    {
      var ct = MediaType.Parse("application/xml, application/xhtml+xml").ToList();
      ShouldBeTestExtensions.ShouldBe(ct[0].MediaType, "application/xhtml+xml");
    }

    [Test]
    public void TheOrderingWorksAsPerRFC2616_14_1()
    {
      var contentTypes = MediaType.Parse("text/plain; q=0.5, text/html, text/x-dvi; q=0.8, text/x-c").ToList();

      ShouldBeTestExtensions.ShouldBe(contentTypes[0].MediaType, "text/x-c");
      ShouldBeTestExtensions.ShouldBe(contentTypes[1].MediaType, "text/html");
      ShouldBeTestExtensions.ShouldBe(contentTypes[2].MediaType, "text/x-dvi");
      ShouldBeTestExtensions.ShouldBe(contentTypes[3].MediaType, "text/plain");
    }
  }
}