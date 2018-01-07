using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using OpenRasta.Codecs;
using OpenRasta.Collections;
using OpenRasta.OperationModel;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  
  static class PipelineDataKeys
  {
    public const string OR_PIPELINE = "__OR_PIPELINE_";

    public const string PIPELINE_STATE = OR_PIPELINE + "PipelineStage";
    public const string HANDLER_TYPE = OR_PIPELINE + "HandlerType";
    public const string RESOURCE_KEY = OR_PIPELINE + "ResourceKey";
    public const string RESPONSE_CODEC = OR_PIPELINE + "ResponseCodec";
    public const string SELECTED_HANDLERS = OR_PIPELINE + "SelectedHandlers";
    public const string SELECTED_RESOURCE = OR_PIPELINE + "SelectedResource";
    public const string OPERATIONS = OR_PIPELINE + "Operations";
    public const string OPERATIONS_ASYNC = "openrasta.operations";

    public const string OWIN_SSL_CLIENT_CERTIFICATE = "ssl.ClientCertificate";
    public const string OWIN_SSL_CLIENT_CERTIFICATE2 = "ssl.ClientCertificate2";
    public const string OWIN_SSL_CLIENT_CERTIFICATE_LOAD = "ssl.LoadClientCertAsync";
  }

  /// <summary>
  /// </summary>
  public class PipelineData : DictionaryBase<object, object>
  {
    public PipelineData()
    {
      PipelineStage = new PipelineStage();
      Owin = new OwinData(this);
    }

    public OwinData Owin { get; }

    /// <summary>
    /// Gets the type of the handler selected when matching a request against the registerd resource.
    /// </summary>
    public Type HandlerType
    {
      get => SafeGet<Type>(PipelineDataKeys.HANDLER_TYPE);
      set => base[PipelineDataKeys.HANDLER_TYPE] = value;
    }

    [Obsolete]
    public IEnumerable<IOperation> Operations
    {
      get => SafeGet<IEnumerable<IOperation>>(PipelineDataKeys.OPERATIONS) ?? Enumerable.Empty<IOperation>();
      set => throw new NotImplementedException();
    }

    public IEnumerable<IOperationAsync> OperationsAsync
    {
      get => SafeGet<IEnumerable<IOperationAsync>>(PipelineDataKeys.OPERATIONS_ASYNC) ??
             Enumerable.Empty<IOperationAsync>();
      set
      {
        base[PipelineDataKeys.OPERATIONS_ASYNC] = value;

#pragma warning disable 618
        base[PipelineDataKeys.OPERATIONS] = value
          .Select(op => new PretendingToBeSyncOperationForLegacyInterceptors(op))
          .Cast<IOperation>()
          .ToList();
#pragma warning restore 618
      }
    }

    /// <summary>
    /// Gets the resource key associated with the requestURI.
    /// </summary>
    public object ResourceKey
    {
      get => SafeGet<object>(PipelineDataKeys.RESOURCE_KEY);
      set => base[PipelineDataKeys.RESOURCE_KEY] = value;
    }

    /// <summary>
    /// Gets the Codec associated with the response entity.
    /// </summary>
    public CodecRegistration ResponseCodec
    {
      get => SafeGet<CodecRegistration>(PipelineDataKeys.RESPONSE_CODEC);
      set => base[PipelineDataKeys.RESPONSE_CODEC] = value;
    }

    public ICollection<IType> SelectedHandlers
    {
      get => SafeGet<ICollection<IType>>(PipelineDataKeys.SELECTED_HANDLERS);
      set => base[PipelineDataKeys.SELECTED_HANDLERS] = value;
    }

    /// <summary>
    /// Provides access to the matched resource registration for a request URI.
    /// </summary>
    public UriRegistration SelectedResource
    {
      get => SafeGet<UriRegistration>(PipelineDataKeys.SELECTED_RESOURCE);
      set => base[PipelineDataKeys.SELECTED_RESOURCE] = value;
    }

    public PipelineStage PipelineStage
    {
      get => SafeGet<PipelineStage>(PipelineDataKeys.PIPELINE_STATE);
      set => base[PipelineDataKeys.PIPELINE_STATE] = value;
    }

    public new object this[object key]
    {
      get => ContainsKey(key) ? base[key] : null;
      set => base[key] = value;
    }

    T SafeGet<T>(string key) where T : class
    {
      return TryGetValue(key, out var o) ? o as T : null;
    }

    public class OwinData
    {
      readonly PipelineData _pipelineData;

      public OwinData(PipelineData pipelineData)
      {
        _pipelineData = pipelineData;
      }

      public X509Certificate SslClientCertificate
      {
        get => _pipelineData.SafeGet<X509Certificate>(PipelineDataKeys.OWIN_SSL_CLIENT_CERTIFICATE);
        set => _pipelineData[PipelineDataKeys.OWIN_SSL_CLIENT_CERTIFICATE] = value;
      }

      public X509Certificate2 SslClientCertificate2
      {
        get => _pipelineData.SafeGet<X509Certificate2>(PipelineDataKeys.OWIN_SSL_CLIENT_CERTIFICATE2);
        set => _pipelineData[PipelineDataKeys.OWIN_SSL_CLIENT_CERTIFICATE2] = value;
      }
      public Func<Task> SslLoadClientCertAsync
      {
        get => _pipelineData.SafeGet<Func<Task>>(PipelineDataKeys.OWIN_SSL_CLIENT_CERTIFICATE_LOAD);
        set => _pipelineData[PipelineDataKeys.OWIN_SSL_CLIENT_CERTIFICATE_LOAD] = value;
      }
    }
  }
}