using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Diagnostics;
using OpenRasta.OperationModel.Diagnostics;
using OpenRasta.Web;

namespace OpenRasta.OperationModel.Filters
{
  public class UriNameOperationFilter : IOperationFilter
  {
    readonly ICommunicationContext _commContext;
    readonly IUriResolver _uris;

    public UriNameOperationFilter(ICommunicationContext commContext, IUriResolver uris)
    {
      _commContext = commContext;
      _uris = uris;
      Log = NullLogger<OperationModelLogSource>.Instance;
      _conventionalOperationName =
        $"{_commContext.Request.HttpMethod}{_commContext.PipelineData.SelectedResource?.UriName}";
    }

    readonly string _conventionalOperationName;

    // ReSharper disable once MemberCanBePrivate.Global - IoC
    public ILogger<OperationModelLogSource> Log { get; set; }

    public IEnumerable<IOperationAsync> Process(IEnumerable<IOperationAsync> operations)
    {
      if (string.IsNullOrEmpty(_commContext.PipelineData.SelectedResource?.UriName))
      {
        return OperationsWhenNoUriName(operations);
      }

      operations = operations.ToList();

      return Match(ByAttribute(operations))
             ?? Match(ByConvention(operations))
             ?? operations;
    }

    static IEnumerable<IOperationAsync> Match(IEnumerable<IOperationAsync> filter)
    {
      var val = filter.ToList();
      return val.Any() ? val : null;
    }

    IEnumerable<IOperationAsync> OperationsWhenNoUriName(IEnumerable<IOperationAsync> operations)
    {
      var filteredOperations = NotConventionalName(NoAttributesOrEmptyUriNameBinding(operations)).ToList();

      if (filteredOperations.Count == operations.Count())
        Log.NoResourceOrUriName();
      else
        Log.FoundOperations(filteredOperations);

      return filteredOperations;
    }

    IEnumerable<IOperationAsync> NotConventionalName(IEnumerable<IOperationAsync> operations)
    {
      var key = _commContext.PipelineData.SelectedResource.ResourceKey;
      if (key == null) return operations;
      
      var method = _commContext.Request.HttpMethod;
      return operations.Where(op =>
        op.Name.StartsWith(method, StringComparison.OrdinalIgnoreCase) == false ||
        _uris.UriNames[key].Contains(op.Name.Substring(method.Length)) == false);
    }

    static IEnumerable<IOperationAsync> NoAttributesOrEmptyUriNameBinding(IEnumerable<IOperationAsync> operations)
    {
      return from operation in operations
        let attribute = operation.FindAttribute<HttpOperationAttribute>()
        where attribute == null || attribute.ForUriName.IsNullOrEmpty()
        select operation;
    }

    IEnumerable<IOperationAsync> ByConvention(IEnumerable<IOperationAsync> operations)
    {
      return from operation in operations
        where string.Compare(operation.Name, _conventionalOperationName, StringComparison.OrdinalIgnoreCase) == 0
        select operation;
    }

    IEnumerable<IOperationAsync> ByAttribute(IEnumerable<IOperationAsync> operations)
    {
      return from operation in operations
        let attribute = operation.FindAttribute<HttpOperationAttribute>()
        where attribute != null
              && attribute.MatchesUriName(_commContext.PipelineData.SelectedResource.UriName)
        select operation;
    }
  }
}