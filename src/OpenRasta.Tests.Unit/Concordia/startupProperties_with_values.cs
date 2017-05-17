using NUnit.Framework;
using OpenRasta.Concordia;
using OpenRasta.Testing;

namespace OpenRasta.Tests.Unit.Concordia
{
    public class startupProperties_with_values
    {
        [Test]
        public void keys_are_correct()
        {
            var props = new StartupProperties {OpenRasta = {Pipeline = {Validate = false}}};
            var shouldBe = props.Properties["openrasta.pipeline.validate"].LegacyShouldBe(false);
        }
    }
}