using System.Linq;
using NUnit.Framework;
using OpenRasta.Codecs;
using OpenRasta.OperationModel;
using OpenRasta.Web;
using Shouldly;

namespace OpenRasta.Tests.Unit.OperationModel.CodecSelectors
{
  public class content_type_no_content_length : requestcodecselector_context
  {
    [Test]
    public void content_type_is_not_set()
    {
      given_filter();
      given_operations();
      Request.Entity = new HttpEntity(){ContentLength = null, ContentType = MediaType.Parse("application/octet-stream").Single()};
      
      given_registration_codec<ApplicationOctetStreamCodec>();
      given_request_httpmethod("GET");
      when_filtering_operations();

      FilteredOperations.All(x => x.GetRequestCodec() == null).ShouldBeTrue();
    }
  }
}