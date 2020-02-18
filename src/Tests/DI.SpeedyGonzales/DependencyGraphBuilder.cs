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
      Nodes = Build(tb).ToList();
      
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

      //
      //
      // var singletonNodes = RewrittenNodes
      //   .Where(node => node.Model.Lifetime == DependencyLifetime.Singleton)
      //   .GroupBy(node=>node.Model.ServiceType)
      //   .SelectMany(nodes => nodes.Select((node, pos) => new
      //   {
      //     node,
      //     pos,
      //     field = tb.DefineField(
      //       node.Model.ServiceType.Name + pos, 
      //       node.Model.ServiceType,
      //       FieldAttributes.Static | FieldAttributes.Private)
      //   }));
      // return new SingletonDefinition
      // {
      //   Type = tb.CreateType(),
      //   
      // }
      // var type = tb.CreateType();
      // var initializers =
      //   fields.Select(f => Expression.Assign(Expression.Field(null, f.field), f.node.FactoryExpression.Body));
      //
      // var init = Expression.Lambda(Expression.Block(initializers));
      
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

    IEnumerable<GraphNode> Build(TypeBuilder tb)
    {
      var transients = _models
        .Where(m=>m.Lifetime == DependencyLifetime.Transient)
        .Select(model => new TransientNode(model, (LambdaExpression) model.Factory)).ToList();
      var singletons = _models
        .Where(m => m.Lifetime == DependencyLifetime.Singleton)
        .GroupBy(m => m.ServiceType)
        .SelectMany(svc =>
          svc.Select((model, position) => new SingletonNode(model, position, tb, (LambdaExpression) model.Factory)));

      var nodes = transients.Cast<GraphNode>().Concat(singletons).ToList();
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