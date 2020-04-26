using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using OpenRasta.Codecs;
using OpenRasta.Collections;
using OpenRasta.OperationModel;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Pipeline
{
  public partial class PipelineData
  {
    public OwinData Owin { get; }

    /// <summary>
    /// Gets the type of the handler selected when matching a request against the registerd resource.
    /// </summary>
    public Type HandlerType
    {
      get => SafeGet<Type>(EnvironmentKeys.HANDLER_TYPE);
      set => base[EnvironmentKeys.HANDLER_TYPE] = value;
    }

    IDictionary<object, object> LegacyKeys
    {
      get => (Dictionary<object, object>) this[EnvironmentKeys.LEGACY_KEYS];
      set => this[EnvironmentKeys.LEGACY_KEYS] = value;
    }

    [Obsolete]
    public IEnumerable<IOperation> Operations
    {
      get => SafeGet<IEnumerable<IOperation>>(EnvironmentKeys.OPERATIONS) ?? Enumerable.Empty<IOperation>();
      set => throw new NotImplementedException();
    }

    public IEnumerable<IOperationAsync> OperationsAsync
    {
      get => SafeGet<IEnumerable<IOperationAsync>>(EnvironmentKeys.OPERATIONS_ASYNC) ??
             Enumerable.Empty<IOperationAsync>();
      set
      {
        base[EnvironmentKeys.OPERATIONS_ASYNC] = value;

#pragma warning disable 618
        IEnumerable<IOperation> legacyOps() => value
          .Select(op => new PretendingToBeSyncOperationForLegacyInterceptors(op))
          .Cast<IOperation>()
          .ToList();
        
        base[EnvironmentKeys.OPERATIONS] = new Memento<IOperation>(legacyOps);
#pragma warning restore 618
      }
    }

    class Memento<T> : IEnumerable<T>
    {
      readonly Lazy<IEnumerable<T>> _lazy;

      public Memento(Func<IEnumerable<T>> result)
      {
        _lazy = new Lazy<IEnumerable<T>>(result,LazyThreadSafetyMode.None);
      }

      public IEnumerator<T> GetEnumerator()
      {
        return _lazy.Value.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }
    }

  /// <summary>
    /// Gets the resource key associated with the requestURI.
    /// </summary>
    public object ResourceKey
    {
      get => SafeGet<object>(EnvironmentKeys.RESOURCE_KEY);
      set => base[EnvironmentKeys.RESOURCE_KEY] = value;
    }

    /// <summary>
    /// Gets the Codec associated with the response entity.
    /// </summary>
    public CodecRegistration ResponseCodec
    {
      get => SafeGet<CodecRegistration>(EnvironmentKeys.RESPONSE_CODEC);
      set => base[EnvironmentKeys.RESPONSE_CODEC] = value;
    }

    public ICollection<IType> SelectedHandlers
    {
      get => SafeGet<ICollection<IType>>(EnvironmentKeys.SELECTED_HANDLERS);
      set => base[EnvironmentKeys.SELECTED_HANDLERS] = value;
    }

    /// <summary>
    /// Provides access to the matched resource registration for a request URI.
    /// </summary>
    public UriRegistration SelectedResource
    {
      get => SafeGet<UriRegistration>(EnvironmentKeys.SELECTED_RESOURCE);
      set => base[EnvironmentKeys.SELECTED_RESOURCE] = value;
    }

    public PipelineStage PipelineStage
    {
      get => SafeGet<PipelineStage>(EnvironmentKeys.PIPELINE_STATE);
      set => base[EnvironmentKeys.PIPELINE_STATE] = value;
    }

    public string RequestUriFileTypeExtension { get; set; }
  }
}