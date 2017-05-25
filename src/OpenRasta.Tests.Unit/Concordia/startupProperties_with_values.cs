using NUnit.Framework;
using OpenRasta.Concordia;
using Shouldly;

namespace OpenRasta.Tests.Unit.Concordia
{
  public class startupProperties_with_values
  {
    [Test]
    public void keys_are_correct()
    {
      var props = new StartupProperties {OpenRasta = {Pipeline = {Validate = false}}};

      props.Properties["openrasta.pipeline.validate"].ShouldBe(false);
    }
  }
}