using NUnit.Framework;
using Shouldly;

namespace OpenRasta.Hosting.AspNet.Tests.Integration
{
    public class finding_types
    {
        [Test]
        public void concrete()
        {
            AspNetHost.FindTypeInOpenRastaProject<SeekedType>().ShouldNotBeNull();
        }

        [Test]
        public void interface_implementation()
        {
            AspNetHost.FindTypeInOpenRastaProject<ISeekedInterface>()
                .ShouldBeOfType<SeekedImplementer>()
                .ShouldNotBeNull();
        }

        public sealed class SeekedType
        {
        }

        public interface ISeekedInterface
        {
        }

        public sealed class SeekedImplementer : ISeekedInterface
        {
        }
    }
}