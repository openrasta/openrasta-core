using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Codecs;
using OpenRasta.DI;
using OpenRasta.Diagnostics;
using OpenRasta.IO;
using OpenRasta.Web;
using OpenRasta.Pipeline;

namespace OpenRasta.Pipeline.Contributors
{
  public class ResponseEntityWriterContributor : KnownStages.IResponseCoding
  {
    readonly IDependencyResolver _resolver;
    static readonly byte[] PADDING = Enumerable.Repeat((byte) ' ', 512).ToArray();

    public ILogger Log { get; } = TraceSourceLogger.Instance;

    public ResponseEntityWriterContributor(IDependencyResolver resolver)
    {
      _resolver = resolver;
    }

    public void Initialize(IPipeline pipeline)
    {
      pipeline.NotifyAsync(WriteResponseBuffered).After<KnownStages.ICodecResponseSelection>();
    }

    async Task<PipelineContinuation> WriteResponseBuffered(ICommunicationContext context)
    {
      if (context.Response.Entity.Instance == null)
      {
        Log.WriteDebug("There was no response entity, not rendering.");
        if (ShouldSendEmptyResponseBody(context))
          await SendEmptyResponse(context);
        return PipelineContinuation.Continue;
      }

      var codecInstance = ResolveCodec(context);
      var writer = CreateWriter(codecInstance);
      using (Log.Operation(this, "Generating response entity."))
      {
        if (context.Response.Entity.Stream.CanSeek)
          await WriteBufferedContent(context, writer);
        else
          await WriteChunkedContent(context, writer);
      }

      return PipelineContinuation.Continue;
    }

    bool ShouldSendEmptyResponseBody(ICommunicationContext context)
    {
      if (!(context.OperationResult is OperationResult.NotFound notFound))
        return true;
      return notFound.Reason != NotFoundReason.NotMapped;
    }

    async Task WriteChunkedContent(ICommunicationContext context,
      Func<object, IHttpEntity, IEnumerable<string>, Task> writer)
    {
      context.Response.WriteHeaders();
      await writer(
        context.Response.Entity.Instance,
        context.Response.Entity,
        context.Request.CodecParameters.ToArray());
    }

    static async Task WriteBufferedContent(ICommunicationContext context,
      Func<object, IHttpEntity, IEnumerable<string>, Task> writer)
    {
      await writer(
        context.Response.Entity.Instance,
        context.Response.Entity,
        context.Request.CodecParameters.ToArray());

      if (context.Response.Entity.Stream.CanSeek)
        context.Response.Entity.ContentLength = context.Response.Entity.Stream.Length;
      await PadErrorMessageForIE(context);

      context.Response.WriteHeaders();
    }

    ICodec ResolveCodec(ICommunicationContext context)
    {
      var codecInstance = FindResponseCodec(context);

      if (context.PipelineData.ResponseCodec?.Configuration != null)
        codecInstance.Configuration = context.PipelineData.ResponseCodec.Configuration;

      return codecInstance;
    }

    ICodec FindResponseCodec(ICommunicationContext context)
    {
      var codecInstance = context.Response.Entity.Codec;
      Type codecType = null;
      if (codecInstance != null)
      {
        Log.WriteDebug("Codec instance with type {0} has already been defined.",
          codecInstance.GetType().Name);
      }
      else if ((codecType = context.PipelineData.ResponseCodec?.CodecType) != null)
      {
        if (_resolver.HasDependency(codecType) == false)
          _resolver.AddDependency(codecType, DependencyLifetime.Transient);

        context.Response.Entity.Codec =
          codecInstance = _resolver.Resolve(codecType) as ICodec;
      }

      if (codecInstance == null)
        throw new CodecNotFoundException(
          $"Codec ({codecType?.Name ?? "null"}) couldn't be initialized.");

      Log.WriteDebug("Codec {0} selected.", codecInstance.GetType().Name);
      return codecInstance;
    }

    static Func<object, IHttpEntity, IEnumerable<string>, Task> CreateWriter(ICodec codecInstance)
    {
      var codecAsync = codecInstance as IMediaTypeWriterAsync;
      if (codecAsync != null) return codecAsync.WriteTo;
      return (instance, entity, parameters) =>
      {
        ((IMediaTypeWriter) codecInstance).WriteTo(instance, entity, parameters.ToArray());
        return Task.CompletedTask;
      };
    }

    Task SendEmptyResponse(ICommunicationContext context)
    {
      Log.WriteDebug("Writing http headers.");
      if (context.Response.StatusCode != 204)
        context.Response.Headers.ContentLength = 0;

      context.Response.WriteHeaders();
      return context.Response.Entity.Stream.FlushAsync();
    }

    static Task PadErrorMessageForIE(ICommunicationContext context)
    {
      if ((context.OperationResult.IsClientError || context.OperationResult.IsServerError)
          && context.Response.Entity.Stream.CanSeek
          && context.Response.Entity.ContentType == MediaType.Html
          && context.Response.Entity.Stream.Length <= 512)
      {
        return context.Response.Entity.Stream.WriteAsync(PADDING, 0,
          (int) (512 - context.Response.Entity.Stream.Length));
      }

      return Task.CompletedTask;
    }
  }
}