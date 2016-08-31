using System.Collections.Generic;

namespace OpenRasta.Pipeline.CallGraph
{
    public interface IGenerateCallGraphs
    {
        IEnumerable<ContributorCall> GenerateCallGraph(IEnumerable<IPipelineContributor> contributors);
    }
}
