using System.Collections.Generic;
using System.Linq;
using OpenRasta.Diagnostics;
using OpenRasta.OperationModel.Filters;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.OperationModel
{
  public interface IOperationFilter : IOperationProcessor //<KnownStages.IOperationFiltering>
  {
  }

  public class CompoundOperationFilter : IOperationFilter
  {
    HttpMethodOperationFilter _httpMethodFilter;
    UriNameOperationFilter _uriNameFilter;
    UriParametersFilter _uriParametersFilter;

    public CompoundOperationFilter(IRequest request, IUriResolver uriResolver, ICommunicationContext context,
      IErrorCollector errorCollector)
    {
      _httpMethodFilter = new HttpMethodOperationFilter(request);

      _uriNameFilter = new UriNameOperationFilter(context, uriResolver);
      _uriParametersFilter = new UriParametersFilter(context, errorCollector);
    }

    public IEnumerable<IOperationAsync> Process(IEnumerable<IOperationAsync> operations)
    {
      var operationsForMethod = _httpMethodFilter.Process(operations).ToList();
      if (operationsForMethod.Count == 0) return Enumerable.Empty<IOperationAsync>();
      var operationsForUriName = _uriNameFilter.Process(operationsForMethod).ToList();
      return operationsForUriName.Count == 0 
        ? Enumerable.Empty<IOperationAsync>() 
        : _uriParametersFilter.Process(operationsForUriName).ToList();
    }
  }
}