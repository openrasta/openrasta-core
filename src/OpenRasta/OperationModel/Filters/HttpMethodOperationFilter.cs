using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Diagnostics;
using OpenRasta.Web;

namespace OpenRasta.OperationModel.Filters
{
  public class HttpMethodOperationFilter : IOperationFilter
  {
    readonly IRequest _request;

    public HttpMethodOperationFilter(IRequest request)
    {
      _request = request;
      Log = NullLogger.Instance;
    }

    // Because of IoC for now
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public ILogger Log { get; set; }

    public IEnumerable<IOperationAsync> Process(IEnumerable<IOperationAsync> operations)
    {
      operations = operations.ToList();
      var operationWithMatchingName = OperationsWithMatchingName(operations).ToList();
      var operationWithMatchingAttribute = OperationsWithMatchingAttribute(operations).ToList();
      Log.WriteDebug("Found {0} operation(s) with a matching name.", operationWithMatchingName.Count);
      Log.WriteDebug("Found {0} operation(s) with matching [HttpOperation] attribute.",
        operationWithMatchingAttribute.Count);
      return operationWithMatchingName.Union(operationWithMatchingAttribute);
    }

    IEnumerable<IOperationAsync> OperationsWithMatchingAttribute(IEnumerable<IOperationAsync> operations)
    {
      return from operation in operations
        let httpAttribute = operation.FindAttribute<HttpOperationAttribute>()
        where httpAttribute != null && httpAttribute.MatchesHttpMethod(_request.HttpMethod)
        select operation;
    }

    private IEnumerable<IOperationAsync> OperationsWithMatchingName(IEnumerable<IOperationAsync> operations) =>
      from operation in operations
      where operation.Name.StartsWith(_request.HttpMethod, StringComparison.OrdinalIgnoreCase)
      select operation;
  }
}