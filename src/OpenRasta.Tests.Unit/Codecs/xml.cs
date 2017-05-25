using Shouldly;

namespace OpenRasta.Tests.Unit.Codecs
{
    using System;
    using System.IO;
    using NUnit.Framework;
    using OpenRasta.Codecs;
    using OpenRasta.Hosting.InMemory;
    using OpenRasta.IO;
    using OpenRasta.Web;
    using OpenRasta.Web.Codecs;

    namespace Tests.Codecs
    {
        public abstract class when_writing_a_xml_representation_context
        {
            InMemoryResponse response;
            protected abstract XmlCodec Codec { get; }

            public void when_I_write(object obj)
            {
                response = new InMemoryResponse();
                Codec.WriteTo(obj, response.Entity, new string[0]);
                ReadResponse(response);
            }

            private void ReadResponse(InMemoryResponse response)
            {
                response.Entity.Stream.Position = 0;
                
                ResponseText = new StreamReader(response.Entity.Stream).ReadToEnd();
            }

            protected string ResponseText { get; set; }

            [Test]
            public void defaults_to_utf8_without_bom()
            {
                when_I_write(new Customer { FirstName = "good text" });
                response.Entity.Stream.Position = 0;
              response.Entity.Stream.ReadByte().ShouldBe('<');
            }
            [Test]
            public void should_write_xml()
            {
                when_I_write(new Customer { FirstName = "good text" });

                Assert.That(ResponseText, Contains.Substring("<FirstName>good text</FirstName>"));
            }

            [Test]
            public void should_handle_invalid_xml_characters_when_writing_xml()
            {
                when_I_write(new Customer { FirstName = "bad text" + Convert.ToChar(0x0b) });

                Assert.That(ResponseText, Contains.Substring("<FirstName>bad text&#xB;</FirstName>"));
            }

            public class Customer
            {
                public string FirstName { get; set; }
            }
        }

        [TestFixture]
        public class when_writing_with_XmlSerializerCodec : when_writing_a_xml_representation_context
        {
            protected override XmlCodec Codec
            {
                get { return new XmlSerializerCodec(); }
            }
        }

        [TestFixture]
        public class when_writing_with_XmlDataContractCodec : when_writing_a_xml_representation_context
        {
            protected override XmlCodec Codec
            {
                get { return new XmlDataContractCodec(); }
            }


        }
    }
}