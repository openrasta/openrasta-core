using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;
using OpenRasta.Binding;
using OpenRasta.Codecs;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.CodecSelectors;
using OpenRasta.OperationModel.Hydrators;
using OpenRasta.Tests.Unit.Fakes;
using OpenRasta.Tests.Unit.Infrastructure;
using OpenRasta.Tests.Unit.OperationModel.Filters;
using OpenRasta.Web;
using Shouldly;

namespace OpenRasta.Tests.Unit.OperationModel.CodecSelectors
{
  public class when_there_is_no_request_entity : requestcodecselector_context
  {
    [Test]
    public void only_operations_already_ready_for_invocation_get_returned()
    {
      given_request_entity_is_zero();
      given_filter();
      given_operations();

      when_filtering_operations();

      FilteredOperations.Count().ShouldBe(3);

      then_operation_should_be_selected("Get");
      codec_is_not_assigned("Get");
      then_operation_should_be_selected("GetWithOptionalValue");
    }
    
    [Test]
    public void only_operations_already_ready_for_invocation_get_returned_for_no_content_length()
    {
      given_filter();
      given_operations();

      when_filtering_operations();

      FilteredOperations.Count().ShouldBe(3);

      then_operation_should_be_selected("Get");
      codec_is_not_assigned("Get");
      then_operation_should_be_selected("GetWithOptionalValue");
    }

    [Test]
    public void only_operations_already_ready_for_invocation_get_returned_for_TRACE_requests()
    {
      given_request_httpmethod("TRACE");
      given_filter();
      given_operations();

      when_filtering_operations();

      FilteredOperations.Count().ShouldBe(3);

      then_operation_should_be_selected("Trace");
      codec_is_not_assigned("Trace");
    }
    
    [Test]
    public void only_operations_already_ready_for_invocation_get_returned_for_TransferEncoding_requests()
    {
      Context.Request.Headers.Add("transfer-encoding","");
      given_request_httpmethod("POST");
      given_request_entity_body("hello world");
      given_filter();
      given_operations();

      when_filtering_operations();

      FilteredOperations.Count().ShouldBe(3);

      then_operation_should_be_selected("Post");
      codec_is_not_assigned("Post");
    }

    void then_operation_should_be_selected(string methodName)
    {
      FilteredOperations.FirstOrDefault(x => x.Name == methodName).ShouldNotBeNull();
    }

    void codec_is_not_assigned(string methodName)
    {
      FilteredOperations.First(x => x.Name == methodName).GetRequestCodec().ShouldBeNull();
    }

    void given_request_entity_is_zero()
    {
      Context.Request.Entity.ContentLength = 0;
    }
  }

  public class when_there_is_no_content_type : requestcodecselector_context
  {
    [Test]
    public void the_content_type_is_set_to_application_octet_stream()
    {
      given_filter();
      given_operations();
      given_request_header_content_type((string) null);
      given_registration_codec<ApplicationOctetStreamCodec>();
      given_request_httpmethod("POST");
      given_request_entity_body(new byte[] {0});

      when_filtering_operations();

      var selectedCodec = FilteredOperations.First(x => x.Name == "PostForStream");
      selectedCodec.GetRequestCodec().CodecRegistration.MediaType.Matches(MediaType.ApplicationOctetStream)
        .ShouldBeTrue();
    }
  }

  public class when_there_is_a_request_entity : requestcodecselector_context
  {
    [Test]
    public void operations_without_any_member_do_not_get_a_codec_assigned()
    {
      given_filter();
      given_operations();
      given_request_header_content_type(MediaType.ApplicationOctetStream);
      given_registration_codec<ApplicationOctetStreamCodec>();
      given_request_entity_body(new byte[] {0});

      when_filtering_operations();

      FilteredOperations.First(x => x.Name == "Get").GetRequestCodec().ShouldBeNull();
    }

    [Test]
    public void operations_with_partially_filled_members_still_get_codec_assigned()
    {
      given_filter();
      given_operations();
      given_request_header_content_type(MediaType.ApplicationXWwwFormUrlencoded);

      given_registration_codec<ApplicationXWwwFormUrlencodedKeyedValuesCodec>();
      given_request_entity_body("firstname=Frodo");

      given_operation_property(x => x.Name == "GetFrodo", "lastname", "baggins");

      when_filtering_operations();

      var requestCodec = FilteredOperations.First(x => x.Name == "GetFrodo").GetRequestCodec();
      requestCodec.ShouldNotBeNull();
      requestCodec.CodecRegistration.CodecType.ShouldBe(typeof(ApplicationXWwwFormUrlencodedKeyedValuesCodec));
    }

    void given_operation_property(Func<IOperationAsync, bool> predicate, string propertyName, string propertyValue)
    {
      Operations.First(predicate).Inputs.First().Binder.SetProperty(
        propertyName,
        new object[] {propertyValue},
        (v, t) => BindingResult.Success(v));
    }
  }

  public class when_a_codec_is_not_found : requestcodecselector_context
  {
    [Test]
    public void the_operation_is_not_selected()
    {
      given_filter();
      given_operations();
      given_request_header_content_type(MediaType.ApplicationOctetStream);
      given_registration_codec<ApplicationOctetStreamCodec>();
      given_request_entity_body(new byte[] {0});

      when_filtering_operations();

      FilteredOperations.FirstOrDefault(x => x.Name == "Post").ShouldBeNull();
    }
  }

  public class requestcodecselector_context : operation_filter_context<CodecSelectorHandler, RequestCodecSelector>
  {
    protected override RequestCodecSelector create_filter()
    {
      return new RequestCodecSelector(Codecs, Context.Request);
    }
  }

  public class CodecSelectorHandler
  {
    public object Get()
    {
      return null;
    }

    public void Post(string value)
    {
    }

    public object Trace()
    {
      return null;
    }

    public void PostForStream(Stream stream)
    {
    }

    public void GetWithOptionalValue([Optional] int optionalIndex)
    {
    }

    public void GetFrodo(Frodo frodo)
    {
    }
  }
}