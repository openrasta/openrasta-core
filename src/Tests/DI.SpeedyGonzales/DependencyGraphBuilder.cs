using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.DI;

namespace Tests.DI.SpeedyGonzales
{
  class DependencyGraphBuilder
  {
    readonly IEnumerable<DependencyFactoryModel> _models;

    public DependencyGraphBuilder(IEnumerable<DependencyFactoryModel> models)
    {
      this._models = models;
      Nodes = Build().ToList();
      RewrittenNodes = RewriteTransients();
      CompileNodes();
    }

    void CompileNodes()
    {
      foreach (var node in RewrittenNodes)
        node.CompileFactory();
    }

    List<GraphNode> RewriteTransients()
    {
      return Nodes.Select(RewriteNode).ToList();
    }

    public List<GraphNode> RewrittenNodes { get; }

    GraphNode RewriteNode(GraphNode node)
    {
      var inputs = node.Inputs.Select(RewriteNode).ToList();
      
      var injectable = node.Inputs
        .Where(x => x.Model.Lifetime == DependencyLifetime.Transient
                    && x.Inputs.Any() == false);
      
      var currentFactory = node.Model.Factory;
      var newFactory = TryRewriteLambdaWithConstructor(currentFactory, injectable);
      if (currentFactory == newFactory) return node;
      return new GraphNode(node.Model, newFactory);
    }

    public List<GraphNode> Nodes { get; set; }

    LambdaExpression TryRewriteLambdaWithConstructor(Expression factory, IEnumerable<GraphNode> newedUp)
    {
      var currentFactory = factory;
      var rewrittenFactory = new ParameterFoldingVisitor(newedUp).Visit(currentFactory);

      return (LambdaExpression)rewrittenFactory;
    }
    IEnumerable<GraphNode> Build()
    {
      var nodes = _models.Select(model => new GraphNode(model, (LambdaExpression)model.Factory)).ToList();
      foreach (var node in nodes)
      {
        node.Inputs = node.Model.Arguments
          .Select(arg => nodes.FirstOrDefault(m => arg.IsAssignableFrom(m.Model.ServiceType)))
          .ToList();
      }

      foreach (var node in nodes)
      {
        node.Dependents = nodes.Where(n => n.Inputs.Contains(node)).ToList();
      }

      return nodes;
    }
  }
}