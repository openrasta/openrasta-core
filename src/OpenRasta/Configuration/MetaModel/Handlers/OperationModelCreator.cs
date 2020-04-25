using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenRasta.Binding;
using OpenRasta.DI;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.OperationModel.MethodBased;
using OpenRasta.TypeSystem;

namespace OpenRasta.Configuration.MetaModel.Handlers
{
  // WIP
  public class OperationModelCreator : IMetaModelHandler
  {
    readonly IEnumerable<IMethodFilter> _methodFilters;
    readonly Func<IEnumerable<IOperationInterceptorAsync>> _asyncInterceptors;
    readonly IObjectBinderLocator _binderLocator;
    readonly IDependencyResolver _resolver;

#pragma warning disable once 618
    readonly Func<IOperation, IEnumerable<IOperationInterceptor>> _syncInterceptors;

    public OperationModelCreator(
      IEnumerable<IMethodFilter> methodFilters,
      Func<IEnumerable<IOperationInterceptorAsync>> asyncInterceptors,
      IObjectBinderLocator binderLocator,
      IDependencyResolver resolver,
      IOperationInterceptorProvider syncInterceptorProvider = null)
    {
      _asyncInterceptors = asyncInterceptors;
      _binderLocator = binderLocator;
      _resolver = resolver;
      _methodFilters = methodFilters.DefaultIfEmpty(new TypeExclusionMethodFilter<object>());
      if (syncInterceptorProvider != null)
        _syncInterceptors = syncInterceptorProvider.GetInterceptors;
      else
        _syncInterceptors = op => Enumerable.Empty<IOperationInterceptor>();
    }

    public void PreProcess(IMetaModelRepository repository)
    {
      var operationsByResource = repository.ResourceRegistrations
        .Select(CreateOperationsForModel)
        .ToLookup(ops => ops.resourceKey, ops => ops.operations);

      MatchOperationsWithUris(repository, operationsByResource);
    }

    void MatchOperationsWithUris(IMetaModelRepository repository, ILookup<object, List<OperationDescriptor>> operationsByResource)
    {
      // This is where the matching of operations and URIs (uri names, parameters, etc) will be moved to.
      foreach (var registration in repository.ResourceRegistrations)
      {
        var operations = operationsByResource[registration.ResourceKey].SelectMany(_ => _).ToList();
        foreach (var uri in registration.Uris)
        {
          foreach (var operation in operations)
            uri.Operations.Add(ToOperationModel(operation));
        }
      }
    }

    (object resourceKey, List<OperationDescriptor> operations) CreateOperationsForModel(ResourceModel resourceModel)
    {
      return (
        resourceModel.ResourceKey,
        MethodBasedOperationCreator.CreateOperationDescriptors(
            resourceModel.Handlers.Select(h => h.Type),
            _asyncInterceptors,
            FilterMethods,
            _syncInterceptors,
            _binderLocator,
            _resolver)
          .ToList());
    }

    IEnumerable<IMethod> FilterMethods(IEnumerable<IMethod> arg)
    {
      return _methodFilters.Aggregate(arg, (methods, filter) => filter.Filter(methods));
    }

    OperationModel ToOperationModel(OperationDescriptor operation)
    {
      return new OperationModel
      {
        Factory = operation.Create,
        HttpMethod = GetHttpMethodForOperation(operation),
        Name = operation.Name,
        Descriptor = operation
      };
    }

    string GetHttpMethodForOperation(OperationDescriptor operation)
    {
      if (operation.HttpOperationAttribute != null)
        return operation.HttpOperationAttribute.Method;

      var match = Regex.Match(operation.Name, "^[A-Z][a-z]*");
      if (match.Success) return match.Value.ToUpperInvariant();
      return operation.Name.ToUpperInvariant();
    }

    public void Process(IMetaModelRepository repository)
    {
    }
  }
}