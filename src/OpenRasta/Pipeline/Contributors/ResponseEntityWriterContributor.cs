using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Codecs;
using OpenRasta.DI;
using OpenRasta.Diagnostics;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.Contributors
{
  // ReSharper disable once ClassNeverInstantiated.Global
  public class ResponseEntityWriterContributor : KnownStages.IResponseCoding
  {
    readonly IDependencyResolver _resolver;

    public ILogger Log { get; } = TraceSourceLogger.Instance;

    public ResponseEntityWriterContributor(IDependencyResolver resolver)
    {
      _resolver = resolver;
    }

    public void Initialize(IPipeline pipeline)
    {
      pipeline.NotifyAsync(WriteResponseEntity);
    }

    async Task<PipelineContinuation> WriteResponseEntity(ICommunicationContext context)
    {
      if (context.Response.Entity.Instance == null)
      {
        Log.WriteDebug("There was no response entity, not rendering.");
        if (ShouldSendEmptyResponseBody(context))
          await SendEmptyResponse(context);
        return PipelineContinuation.Continue;
      }

      var isBuffered = context.PipelineData.ResponseCodec.CodecModel?.IsBuffered;
      var entity = context.Response.Entity;
      BufferedHttpEntity buffered = null;
      
      if (isBuffered == true) entity = buffered = new BufferedHttpEntity(entity);

      var codecInstance = ResolveCodec(context);
      var writer = CreateWriter(codecInstance);
      using (Log.Operation(this, "Generating response entity."))
      {
        await writer(
          entity.Instance,
          entity,
          context.Request.CodecParameters.ToArray());

        await entity.Stream.FlushAsync();
        
        if (isBuffered == true)
        {
          await buffered.SendResponseAsync();
        }
      }

      return PipelineContinuation.Continue;
    }

    bool ShouldSendEmptyResponseBody(ICommunicationContext context)
    {
      return Is404NotMapped(context) == false;
    }

    bool Is404NotMapped(ICommunicationContext context)
    {
      return context.OperationResult is OperationResult.NotFound notFound &&
             notFound.Reason == NotFoundReason.NotMapped;
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
      if (codecInstance is IMediaTypeWriterAsync codecAsync) return codecAsync.WriteTo;
      return (instance, entity, parameters) =>
      {
        ((IMediaTypeWriter) codecInstance).WriteTo(instance, entity, parameters.ToArray());
        return Task.CompletedTask;
      };
    }

    async Task SendEmptyResponse(ICommunicationContext context)
    {
      Log.WriteDebug("Writing http headers.");
      if (context.Response.StatusCode != 204)
        context.Response.Headers.ContentLength = 0;

      await context.Response.Entity.Stream.FlushAsync();
    }

    class BufferedHttpEntity : IHttpEntity
    {
      readonly IHttpEntity _httpEntityImplementation;

      public BufferedHttpEntity(IHttpEntity httpEntityImplementation)
      {
        _httpEntityImplementation = httpEntityImplementation;
      }

      public void Dispose()
      {
        _httpEntityImplementation.Dispose();
      }

      public ICodec Codec
      {
        get => _httpEntityImplementation.Codec;
        set => _httpEntityImplementation.Codec = value;
      }

      public object Instance
      {
        get => _httpEntityImplementation.Instance;
        set => _httpEntityImplementation.Instance = value;
      }

      public MediaType ContentType
      {
        get => _httpEntityImplementation.ContentType;
        set => _httpEntityImplementation.ContentType = value;
      }

      public long? ContentLength
      {
        get => _httpEntityImplementation.ContentLength;
        set => _httpEntityImplementation.ContentLength = value;
      }

      public HttpHeaderDictionary Headers => _httpEntityImplementation.Headers;

      public Stream Stream => new MemoryStream();

      public IList<Error> Errors => _httpEntityImplementation.Errors;


      public async Task SendResponseAsync()
      {
        await Stream.CopyToAsync(_httpEntityImplementation.Stream);
      }
    }
  }
}