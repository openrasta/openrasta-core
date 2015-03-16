using OpenRasta.DI;

namespace OpenRasta.Pipeline.CallGraph
{
    internal class CallGraphGeneratorFactory
    {
        private readonly IDependencyResolver _dependencyResolver;

        public CallGraphGeneratorFactory(IDependencyResolver dependencyResolver)
        {
            _dependencyResolver = dependencyResolver;
        }

        public IGenerateCallGraphs GetCallGraphGenerator()
        {
            return _dependencyResolver.HasDependency<IGenerateCallGraphs>()
                ? _dependencyResolver.Resolve<IGenerateCallGraphs>()
                : new DefaultCallGraphGenerator();
        }
    }
}
