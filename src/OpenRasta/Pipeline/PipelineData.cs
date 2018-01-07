using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using OpenRasta.Codecs;
using OpenRasta.Collections;
using OpenRasta.OperationModel;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  /// <summary>
  /// </summary>
  public class PipelineData : DictionaryBase<object, object>
  {
    const string PIPELINE_STATE = OR_PIPELINE + "PipelineStage";
    const string HANDLER_TYPE = OR_PIPELINE + "HandlerType";
    const string OR_PIPELINE = "__OR_PIPELINE_";
    const string RESOURCE_KEY = OR_PIPELINE + "ResourceKey";
    const string RESPONSE_CODEC = OR_PIPELINE + "ResponseCodec";
    const string SELECTED_HANDLERS = OR_PIPELINE + "SelectedHandlers";
    const string SELECTED_RESOURCE = OR_PIPELINE + "SelectedResource";
    const string OPERATIONS = OR_PIPELINE + "Operations";
    const string OPERATIONS_ASYNC = "openrasta.operations";

    const string SSL_CLIENT_CERTIFICATE = "ssl.ClientCertificate";

    public PipelineData()
    {
      PipelineStage = new PipelineStage();
    }

    /// <summary>
    /// Gets the type of the handler selected when matching a request against the registerd resource.
    /// </summary>
    public Type HandlerType
    {
      get => SafeGet<Type>(HANDLER_TYPE);
      set => base[HANDLER_TYPE] = value;
    }

    public X509Certificate ClientCertificate
    {
      get => SafeGet<X509Certificate>(SSL_CLIENT_CERTIFICATE);
      set => base[SSL_CLIENT_CERTIFICATE] = value;
    }

    [Obsolete]
    public IEnumerable<IOperation> Operations
    {
      get => SafeGet<IEnumerable<IOperation>>(OPERATIONS) ?? Enumerable.Empty<IOperation>();
      set => throw new NotImplementedException();
    }

    public IEnumerable<IOperationAsync> OperationsAsync
    {
      get => SafeGet<IEnumerable<IOperationAsync>>(OPERATIONS_ASYNC) ?? Enumerable.Empty<IOperationAsync>();
      set
      {
        base[OPERATIONS_ASYNC] = value;

#pragma warning disable 618
        base[OPERATIONS] = value
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
      get => SafeGet<object>(RESOURCE_KEY);
      set => base[RESOURCE_KEY] = value;
    }

    /// <summary>
    /// Gets the Codec associated with the response entity.
    /// </summary>
    public CodecRegistration ResponseCodec
    {
      get => SafeGet<CodecRegistration>(RESPONSE_CODEC);
      set => base[RESPONSE_CODEC] = value;
    }

    public ICollection<IType> SelectedHandlers
    {
      get => SafeGet<ICollection<IType>>(SELECTED_HANDLERS);
      set => base[SELECTED_HANDLERS] = value;
    }

    /// <summary>
    /// Provides access to the matched resource registration for a request URI.
    /// </summary>
    public UriRegistration SelectedResource
    {
      get => SafeGet<UriRegistration>(SELECTED_RESOURCE);
      set => base[SELECTED_RESOURCE] = value;
    }

    public PipelineStage PipelineStage
    {
      get => SafeGet<PipelineStage>(PIPELINE_STATE);
      set => base[PIPELINE_STATE] = value;
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
  }
}