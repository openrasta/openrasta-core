using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
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
      RewrittenNodes = RewriteNodes();
      CompileNodes();
      DoStuff();
    }

    void CompileNodes()
    {
      foreach (var node in RewrittenNodes)
        node.CompileFactory();
    }

    List<GraphNode> RewriteNodes()
    {
      return Nodes.Select(FoldTransients).ToList();
    }

    void DoStuff()
    {
      var typeSignature = "MyDynamicType";
      var an = new AssemblyName("SpeedyGonzalesStaticTypes");
      var assemblyBuilder =
        AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
      var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
      var tb = moduleBuilder.DefineType("Singletons",
        TypeAttributes.Public |
        TypeAttributes.Class |
        TypeAttributes.AutoClass |
        TypeAttributes.AnsiClass |
        TypeAttributes.BeforeFieldInit |
        TypeAttributes.AutoLayout,
        null);

      var singletonNodes = RewrittenNodes.Where(node =>
        node.Model.Lifetime == DependencyLifetime.Singleton && node.FactoryExpression.Parameters.Any() == false);

      var fields = singletonNodes.Select(node => new
      {
        node,
        field = tb.DefineField(
          node.Model.ServiceType.Name + node.Model.ConcreteType.Name, node.Model.ServiceType,
          FieldAttributes.Static | FieldAttributes.Private)
      }).ToList();

      var type = tb.CreateType();
      var initializers =
        fields.Select(f => Expression.Assign(Expression.Field(null, f.field), f.node.FactoryExpression.Body));

      var init = Expression.Lambda(Expression.Block(initializers));
      
    }

    
    public List<GraphNode> RewrittenNodes { get; }

    GraphNode FoldTransients(GraphNode node)
    {
      var inputs = node.Inputs.Select(FoldTransients).ToList();

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
      return (LambdaExpression) rewrittenFactory;
    }

    IEnumerable<GraphNode> Build()
    {
      var nodes = _models.Select(model => new GraphNode(model, (LambdaExpression) model.Factory)).ToList();
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