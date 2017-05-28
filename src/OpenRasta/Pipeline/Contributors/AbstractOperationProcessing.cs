using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.DI;
using OpenRasta.OperationModel;
using OpenRasta.Web;

namespace OpenRasta.Pipeline.Contributors
{
    public abstract class AbstractOperationProcessing<TProcessor, TStage> where TProcessor : IOperationProcessor<TStage>
        where TStage : IPipelineContributor
    {
      protected AbstractOperationProcessing(IDependencyResolver resolver)
        {
        }
    }
}