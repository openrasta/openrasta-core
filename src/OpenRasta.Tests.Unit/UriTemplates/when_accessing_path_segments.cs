using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;

namespace OpenRasta.Tests.Unit.UriTemplates
{
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
}