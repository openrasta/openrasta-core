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
  public class OperationModelCreator : IMetaModelHandler
  {
    IEnumerable<IOperationInterceptorAsync> _asyncInterceptors;
    Func<IEnumerable<IMethod>, IEnumerable<IMethod>> _filters;
    Func<IOperation, IEnumerable<IOperationInterceptor>> _syncInterceptors;
    IObjectBinderLocator _binderLocator;
    IDependencyResolver _resolver;

    public OperationModelCreator()
    {
    }
    public void PreProcess(IMetaModelRepository repository)
    {
      foreach (var resourceModel in repository.ResourceRegistrations)
        CreateOperationsForModel(resourceModel);
    }

    void CreateOperationsForModel(ResourceModel resourceModel)
    {
      var operations = MethodBasedOperationCreator.CreateOperationDescriptors(
        resourceModel.Handlers.Select(h => h.Type),
        _asyncInterceptors,
        _filters,
        _syncInterceptors,
        _binderLocator,
        _resolver).ToList();
      foreach(var uri in resourceModel.Uris)
      foreach (var operation in operations)
        uri.Operations.Add(ToOperationModel(operation));
        
    }

    OperationModel ToOperationModel(OperationDescriptor operation)
    {
      return new OperationModel()
      {
        Factory = operation.Create,
        HttpMethod = GetHttpMethodForOperation(operation),
        Name = operation.Name
      };
    }

    string GetHttpMethodForOperation(OperationDescriptor operation)
    {
      if (operation.HttpOperationAttribute != null)
        return operation.HttpOperationAttribute.Method;
      
      var match = Regex.Match(operation.Name,"^[A-Z][a-z]*");
      if (match.Success) return match.Value.ToUpperInvariant();
      return operation.Name.ToUpperInvariant();
    }

    public void Process(IMetaModelRepository repository)
    {
    }
  }
}