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

        public UriNameOperationFilter(ICommunicationContext commContext)
        {
            _commContext = commContext;
            Log = NullLogger<OperationModelLogSource>.Instance;
        }

        public ILogger<OperationModelLogSource> Log { get; set; }

        public IEnumerable<IOperationAsync> Process(IEnumerable<IOperationAsync> operations)
        {
            if (string.IsNullOrEmpty(_commContext.PipelineData.SelectedResource?.UriName))
            {
                Log.NoResourceOrUriName();
                return operations;
            }

            var attribOperations = OperationsWithMatchingAttribute(operations).ToList();
            Log.FoundOperations(attribOperations);
            return attribOperations.Count > 0 ? attribOperations : operations;
        }

        IEnumerable<IOperationAsync> OperationsWithMatchingAttribute(IEnumerable<IOperationAsync> operations)
        {
            return from operation in operations
                   let attribute = operation.FindAttribute<HttpOperationAttribute>()
                   where attribute != null
                         && attribute.MatchesUriName(_commContext.PipelineData.SelectedResource.UriName)
                   select operation;
        }
    }
}