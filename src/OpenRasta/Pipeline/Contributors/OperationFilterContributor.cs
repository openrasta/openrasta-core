using System.Collections.Generic;
using System.Linq;
using OpenRasta.DI;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Filters;
using OpenRasta.Pipeline;

namespace OpenRasta.Pipeline.Contributors
{
    public class OperationFilterContributor :
        AbstractOperationProcessing<IOperationFilter, KnownStages.IOperationFiltering>,
        KnownStages.IOperationFiltering
    {
        public OperationFilterContributor(IDependencyResolver resolver) : base(resolver)
        {
        }

        protected override void InitializeWhen(IPipelineExecutionOrder pipeline)
        {
            pipeline.After<KnownStages.IOperationCreation>();
        }

        protected override IEnumerable<IOperationFilter> OrderProcessors(IEnumerable<IOperationFilter> operations) =>
            operations.OrderBy(operation =>
            {
                switch (operation)
                {
                    case HttpMethodOperationFilter _: return 0;
                    case UriNameOperationFilter _: return 1;
                    case UriParametersFilter _: return 2;
                    default: return 3;
                }
            });
    }
}