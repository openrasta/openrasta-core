using System;
using NUnit.Framework;
using OpenRasta.Codecs;
using OpenRasta.Pipeline.Contributors;
using OpenRasta.Tests;
using OpenRasta.Tests.Unit.Fakes;
using OpenRasta.Web;
using OpenRasta.Pipeline;
using OpenRasta.Tests.Unit.Infrastructure;
using Shouldly;

namespace ResponseEntityCodecResolver_Specification
{
  public class when_a_codec_is_already_defined : openrasta_context
  {
    [Test]
    public void the_codec_is_not_changed_and_the_pipeline_continues()
    {
      given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
      given_response_entity(new Customer(), typeof(Codec), "application/xml");
      given_registration_codec<CustomerCodec, Customer>("text/plain");
      given_request_header_accept("text/plain");

      when_sending_notification<KnownStages.IOperationResultInvocation>().ShouldBe(PipelineContinuation.Continue);

      Context.PipelineData.ResponseCodec.CodecType.ShouldBe(typeof(Codec));
    }

    [Test]
    public void there_is_no_vary_header()
    {
      given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
      given_response_entity(new Customer(), typeof(Codec), "application/xml");
      given_registration_codec<CustomerCodec, Customer>("text/plain");
      given_request_header_accept("text/plain");

      when_sending_notification<KnownStages.IOperationResultInvocation>().ShouldBe(PipelineContinuation.Continue);

      Context.Response.Entity.Headers["Vary"].ShouldBeNull();
    }
  }

  public class when_no_codec_has_been_predefined_defined : openrasta_context
  {
    [Test]
    public void a_semi_wildcard_gets_priority_over_a_full_wildcard()
    {
      given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
      given_response_entity(new Customer());
      given_registration_codec<CustomerCodec, Customer>("text/plain");
      given_registration_codec<AnotherCustomerCodec, Customer>("application/xml");
      given_request_header_accept("text/*,*/*");

      when_running_pipeline();

      Context.PipelineData.ResponseCodec.CodecType.ShouldBe(typeof(CustomerCodec));
      Context.Response.Entity.ContentType.MediaType.ShouldBe("text/plain");
    }

    [Test]
    public void bad_request_is_returned_when_the_accept_header_has_only_one_entry_that_is_invalid()
    {
      given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
      given_response_entity(new Customer());
      given_registration_codec<CustomerCodec, Customer>("text/plain");
      given_request_header_accept("q=");

      when_sending_notification<KnownStages.IOperationResultInvocation>().ShouldBe(PipelineContinuation.RenderNow);

      Context.OperationResult.ShouldBeAssignableTo<OperationResult.BadRequest>();
      Context.Response.Headers["Warning"].ShouldBe("199 Malformed accept header");
      Context.Response.Headers["Vary"].ShouldBeNull();
    }

    [Test]
    public void ignore_an_invalid_entry_if_accept_header_has_mixture_of_valid_and_invalid_values()
    {
      given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
      given_response_entity(new Customer());
      given_registration_codec<CustomerCodec, Customer>("text/plain");
      given_request_header_accept("text/plain,q=");

      when_running_pipeline();

      Context.PipelineData.ResponseCodec.CodecType.ShouldBe(typeof(CustomerCodec));
      Context.Response.Entity.ContentType.MediaType.ShouldBe("text/plain");
      Context.Response.Headers["Vary"].ShouldBe("Accept");

    }

    [Test]
    public void an_error_is_returned_when_no_suitable_codec_is_found()
    {
      given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
      given_response_entity(new Customer());
      given_registration_codec<CustomerCodec, Customer>("text/plain");
      given_request_header_accept("application/xml");

      when_sending_notification<KnownStages.IOperationResultInvocation>().ShouldBe(PipelineContinuation.RenderNow);


      Context.OperationResult.ShouldBeAssignableTo<OperationResult.ResponseMediaTypeUnsupported>();
    }

    [Test]
    public void nothing_happens_for_a_null_entity()
    {
      given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
      given_response_entity(null);
      given_registration_codec<CustomerCodec, Customer>("text/plain");
      given_request_header_accept("text/plain");

      when_running_pipeline();

      Context.PipelineData.ResponseCodec.ShouldBeNull();
      Context.Response.Entity.ContentType.ShouldBeNull();
    }

    [Test]
    public void the_codec_is_selected_for_an_exact_match()
    {
      given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
      given_response_entity(new Customer());
      given_registration_codec<CustomerCodec, Customer>("text/plain");
      given_registration_codec<Codec, Customer>("application/xml");
      given_request_header_accept("text/plain");

      when_running_pipeline();

      Context.PipelineData.ResponseCodec.CodecType.ShouldBe(typeof(CustomerCodec));
      Context.Response.Entity.ContentType.MediaType.ShouldBe("text/plain");
    }

    [Test]
    public void the_server_quality_is_respected_when_the_accept_header_has_a_full_wildcard()
    {
      given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
      given_response_entity(new Customer());
      given_registration_codec<CustomerCodec, Customer>("text/plain;q=0.9");
      given_registration_codec<AnotherCustomerCodec, Customer>("application/xml");
      given_request_header_accept("*/*");
      when_running_pipeline();


      Context.PipelineData.ResponseCodec.CodecType.ShouldBe(typeof(AnotherCustomerCodec));
      Context.Response.Entity.ContentType.MediaType.ShouldBe("application/xml");
    }

    [Test]
    public void codec_wildcard_and_no_accept_gives_octet_stream()
    {
      
      given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
      given_response_entity(new Customer());
      given_registration_codec<CustomerCodec, Customer>("*/*");

      when_running_pipeline();

      Context.PipelineData.ResponseCodec.CodecType.ShouldBe(typeof(CustomerCodec));
      Context.Response.Entity.ContentType.MediaType.ShouldBe("application/octet-stream");
    }
       
    [Test]
    public void codec_wildcard_gives_client_with_wildcard_accept_app_octet_stream()
    {
      given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
      given_response_entity(new Customer());
      given_registration_codec<CustomerCodec, Customer>("*/*");
      given_request_header_accept("*/*");

      when_running_pipeline();

      Context.PipelineData.ResponseCodec.CodecType.ShouldBe(typeof(CustomerCodec));
      Context.Response.Entity.ContentType.MediaType.ShouldBe("application/octet-stream");
    }
    [Test]
    public void codec_wildcard_gives_client_any_requested_mediatype()
    {
      given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
      given_response_entity(new Customer());
      given_registration_codec<CustomerCodec, Customer>("*/*");
      given_request_header_accept("text/plain");

      when_running_pipeline();

      Context.PipelineData.ResponseCodec.CodecType.ShouldBe(typeof(CustomerCodec));
      Context.Response.Entity.ContentType.MediaType.ShouldBe("text/plain");
    }
    [Test]
    public void the_client_quality_parameter_is_respected()
    {
      given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
      given_response_entity(new Customer());
      given_registration_codec<CustomerCodec, Customer>("text/plain;q=0.9");
      given_registration_codec<AnotherCustomerCodec, Customer>("application/xml");
      given_request_header_accept("text/plain,application/xml;q=0.1");

      when_running_pipeline();

      Context.PipelineData.ResponseCodec.CodecType.ShouldBe(typeof(CustomerCodec));
      Context.Response.Entity.ContentType.MediaType.ShouldBe("text/plain");
    }

    void when_running_pipeline()
    {
      when_sending_notification<KnownStages.IOperationResultInvocation>().ShouldBe(PipelineContinuation.Continue);
    }

    [Test, Category("Regression")]
    public void the_server_quality_is_used_to_select_the_closest_media_type()
    {
      // Regression from ticket #54
      given_pipeline_contributor<ResponseEntityCodecResolverContributor>();
      given_response_entity(new Customer());
      given_registration_codec<CustomerCodec, Customer>("application/xhtml+xml;q=0.9,text/html,application/vnd.openrasta.htmlfragment+xml;q=0.5");
      given_request_header_accept("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

      when_sending_notification<KnownStages.IOperationResultInvocation>().ShouldBe(PipelineContinuation.Continue);

      Context.PipelineData.ResponseCodec.CodecType.ShouldBe(typeof(CustomerCodec));
      Context.Response.Entity.ContentType.MediaType.ShouldBe("text/html");
    }
  }
}