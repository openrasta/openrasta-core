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
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public ILogger<OperationModelLogSource> Log { get; set; }

    public IEnumerable<IOperationAsync> Process(IEnumerable<IOperationAsync> operations)
    {
      return ProcessOperationsList(operations.ToList());
    }

    IEnumerable<IOperationAsync> ProcessOperationsList(List<IOperationAsync> operations)
    {
      return string.IsNullOrEmpty(_commContext.PipelineData.SelectedResource?.UriName)
        ? OperationsWhenNoUriName(operations) 
        : OperationsWhenUriName(operations);
    }

    List<IOperationAsync> OperationsWhenUriName(List<IOperationAsync> operations)
    {
      return Match(operations.FindByAttributeUriName(_commContext.PipelineData.SelectedResource.UriName).ToList())
             ?? Match(operations.FindByOperationName(_conventionalOperationName).ToList())
             ?? operations;
    }

    static List<IOperationAsync> Match(List<IOperationAsync> val)
    {
      return val.Any() ? val : null;
    }

    List<IOperationAsync> OperationsWhenNoUriName(List<IOperationAsync> operations)
    {
      var filteredOperations = NotConventionalName(
        NoAttributesOrNoUriName(operations).ToList());

      if (filteredOperations.Count == operations.Count())
        Log.NoResourceOrUriName();
      else
        Log.FoundOperations(filteredOperations);

      return filteredOperations;
    }

    List<IOperationAsync> NotConventionalName(List<IOperationAsync> operations)
    {
      var key = _commContext.PipelineData.SelectedResource.ResourceKey;
      if (key == null) return operations;
      
      var method = _commContext.Request.HttpMethod;
      return operations
        .Where(op =>
        op.Name.StartsWith(method, StringComparison.OrdinalIgnoreCase) == false ||
        _uris.UriNames.TryGetValue(key, out var namesForResourceKey) == false ||
        namesForResourceKey.Contains(op.Name.Substring(method.Length)) == false)
        .ToList();
    }

    static IEnumerable<IOperationAsync> NoAttributesOrNoUriName(List<IOperationAsync> operations)
    {
      return from operation in operations
        let attribute = operation.FindAttribute<HttpOperationAttribute>()
        where attribute == null || attribute.ForUriName.IsNullOrEmpty()
        select operation;
    }

  }

  public static class OperationListExtensions
  {
    public static IEnumerable<IOperationAsync> FindByOperationName(this IEnumerable<IOperationAsync> operations, string operationName)
    {
      return from operation in operations
        where string.Compare(operation.Name, operationName, StringComparison.OrdinalIgnoreCase) == 0
        select operation;
    }

    public static IEnumerable<IOperationAsync> FindByAttributeUriName(this IEnumerable<IOperationAsync> operations, string uriName)
    {
      return from operation in operations
        let attribute = operation.FindAttribute<HttpOperationAttribute>()
        where attribute != null
              && attribute.MatchesUriName(uriName)
        select operation;
    }
  }
}