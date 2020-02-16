using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using OpenRasta.Configuration;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.DI;
using Shouldly;
using Xunit;

namespace Tests.DI.SpeedyGonzales
{
  public class ScopeCache
  {
    readonly Dictionary<Type, Func<object>> _instances = new Dictionary<Type, Func<object>>();

    public bool TryGetInstance<T>(out T instance)
    {
      if (_instances.TryGetValue(typeof(T), out var factory))
      {
        instance = (T) factory();
        return true;
      }

      instance = default;
      return false;
    }

    public void StoreInstance(Type serviceType, Func<object> instance)
    {
      _instances.Add(serviceType, instance);
    }
  }

  public class SpeedyGonzalesResolver : IEnumerable
  {
    public List<DependencyFactoryModel> Registrations { get; } = new List<DependencyFactoryModel>();

    readonly ScopeCache _singletons = new ScopeCache();
    readonly ScopeCache _transient = new ScopeCache();

    public void Add(Action<ITypeRegistrationContext> registration)
    {
      var typeRegistration = new TypeRegistrationContext();
      registration(typeRegistration);
      Registrations.Add(typeRegistration.Model);
    }

    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

    public T Resolve<T>()
    {
      return _singletons.TryGetInstance<T>(out var result)
        ? result
        : _transient.TryGetInstance<T>(out var instanceResult)
          ? instanceResult
          : throw new DependencyResolutionException();
    }

    public void Seal()
    {
      var dependencies = new DependencyGraphBuilder(Registrations).RewrittenNodes;

      
      foreach (var reg in dependencies.Select(d=>d.Model))
      {
        if (reg.Lifetime == DependencyLifetime.Singleton)
        {
          var instance = reg.UntypedFactory(null);
          _singletons.StoreInstance(reg.ServiceType, () => instance);
        }
        else if (reg.Lifetime == DependencyLifetime.Transient)
          _transient.StoreInstance(reg.ServiceType, () => reg.UntypedFactory(null));
      }
    }
  }

  class ParameterFoldingVisitor : ExpressionVisitor
  {
    readonly IEnumerable<GraphNode> replacements;
    Dictionary<ParameterExpression, GraphNode> parametersToReplace;

    public ParameterFoldingVisitor(IEnumerable<GraphNode> replacements)
    {
      this.replacements = replacements;
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
      parametersToReplace =
        (from param in node.Parameters
          let replacementNode = replacements.LastOrDefault(r => param.Type.IsAssignableFrom(r.Model.ServiceType))
          where replacementNode != null
          select new {param, replacementNode})
        .ToDictionary(x => x.param, x => x.replacementNode);
      if (parametersToReplace.Any() == false) return node;
      var rewrittenBody = Visit(node.Body);
      return Expression.Lambda(rewrittenBody, node.Parameters.Except(parametersToReplace.Keys));
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
      if (parametersToReplace.ContainsKey(node))
      {
        var lambda = (LambdaExpression) parametersToReplace[node].Model.Factory;
        if (lambda.Parameters.Any()) throw new InvalidOperationException();
        return lambda.Body;
      }

      return base.VisitParameter(node);
    }
  }

  class DependencyGraphBuilder
  {
    readonly IEnumerable<DependencyFactoryModel> _models;

    public DependencyGraphBuilder(IEnumerable<DependencyFactoryModel> models)
    {
      this._models = models;
      Nodes = Build().ToList();
      RewrittenNodes = Nodes.Select(RewriteNode).ToList();
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

  public class GraphNode
  {
    public GraphNode(DependencyFactoryModel model, LambdaExpression factoryExpression)
    {
      Model = model;
      FactoryExpression = factoryExpression;
    }

    public Func<object> Factory { get; set; }

    public DependencyFactoryModel Model { get; set; }

    public List<GraphNode> Inputs { get; set; }

    public List<GraphNode> Dependents { get; set; }
    
    public LambdaExpression FactoryExpression { get;  }
    
    
  }

  public class singleton_dependency
  {
  }

  public class graph
  {
    public graph()
    {
    }
  }

  public class transient_constructor_folding
  {
    [Fact]
    public void transient_constructors_get_rewritten()
    {
      var container = new SpeedyGonzalesResolver
      {
        {context => context.Transient((Hero hero) => new Quest(hero,null))},
        {context => context.Transient(() => new Hero("Heracles"))}
      };
      container.Seal();
    }
  }

  
  public class transient_rewriting
  {
    DependencyGraphBuilder graph;

    public transient_rewriting()
    {
      graph = new DependencyGraphBuilder(new SpeedyGonzalesResolver()
      {
        context => context.Transient((Hero hero, Artifact artifact) => new Quest(hero, artifact)),
        context => context.Transient(() => new Hero("Heracles")),
        context => context.Transient(() => new Artifact("Hydra"))
      }.Registrations);
    }

    [Fact]
    public void has_three_original_nodes()
    {
      graph.Nodes[0].ShouldBe<Quest>("(hero, artifact) => new Quest(hero, artifact)");
      graph.Nodes[1].ShouldBe<Hero>("() => new Hero(\"Heracles\")");
      graph.Nodes[2].ShouldBe<Artifact>("() => new Artifact(\"Hydra\")");
    }

    [Fact]
    public void transients_are_rewritten()
    {
      graph.RewrittenNodes[0].ShouldBe<Quest>("() => new Quest(new Hero(\"Heracles\"), new Artifact(\"Hydra\"))");
      graph.RewrittenNodes[1].ShouldBe<Hero>("() => new Hero(\"Heracles\")");
      graph.RewrittenNodes[2].ShouldBe<Artifact>("() => new Artifact(\"Hydra\")");
    }
  }

  public class singleton_rewriting
  {
    DependencyGraphBuilder graph;

    public singleton_rewriting()
    {
      graph = new DependencyGraphBuilder(new SpeedyGonzalesResolver()
      {
        context => context.Transient((Hero hero, Artifact artifact) => new Quest(hero, artifact)),
        context => context.Singleton(() => new Hero("Heracles")),
        context => context.Transient(() => new Artifact("Hydra"))
      }.Registrations);
    }

    [Fact]
    public void has_three_original_nodes()
    {
      graph.Nodes[0].ShouldBe<Quest>("(hero, artifact) => new Quest(hero, artifact)");
      graph.Nodes[1].ShouldBe<Hero>("() => new Hero(\"Heracles\")");
      graph.Nodes[2].ShouldBe<Artifact>("() => new Artifact(\"Hydra\")");
    }

    [Fact]
    public void transients_are_rewritten()
    {
      graph.RewrittenNodes[0].ShouldBe<Quest>("hero => new Quest(hero, new Artifact(\"Hydra\"))");
      graph.RewrittenNodes[1].ShouldBe<Hero>("() => new Hero(\"Heracles\")");
      graph.RewrittenNodes[2].ShouldBe<Artifact>("() => new Artifact(\"Hydra\")");
    }
  }
  public static class GraphTestExtensions
  {
    public static void ShouldBe<TService>(this GraphNode node, string factory)
    {
      node.Model.ServiceType.ShouldBe(typeof(TService));
      node.FactoryExpression.ToString().ShouldBe(factory);
    }
  }

  public class Artifact
  {
    public string Name { get; }

    public Artifact(string name)
    {
      Name = name;
    }
  }

  public class singleton
  {
    SpeedyGonzalesResolver container;

    public singleton()
    {
      container = new SpeedyGonzalesResolver
      {
        {context => context.Singleton(() => new SimpleService())}
      };
      container.Seal();
    }

    [Fact]
    public void is_resolved()
    {
      var instance1 = container.Resolve<SimpleService>();
      var instance2 = container.Resolve<SimpleService>();
      instance1.ShouldBeSameAs(instance2);
    }
  }

  public class transient
  {
    SpeedyGonzalesResolver container;

    public transient()
    {
      container = new SpeedyGonzalesResolver
      {
        {context => context.Transient(() => new SimpleService())}
      };
      container.Seal();
    }

    [Fact]
    public void is_resolved()
    {
      var instance1 = container.Resolve<SimpleService>();
      var instance2 = container.Resolve<SimpleService>();
      instance1.ShouldNotBeSameAs(instance2);
    }
  }

  public class dependency
  {
    SpeedyGonzalesResolver container;

    public dependency()
    {
      container = new SpeedyGonzalesResolver
      {
        context => context.Transient((Hero hero) => new Quest(hero,null)),
        context => context.Transient(() => new Hero("Heracles"))
      };
      container.Seal();
    }

    [Fact]
    public void is_resolved()
    {
      var firstQuest = container.Resolve<Quest>();
      var secondQuest = container.Resolve<Quest>();
      var hero = container.Resolve<Hero>();
      firstQuest.ShouldNotBeSameAs(secondQuest);
      firstQuest.Hero.ShouldBeSameAs(hero);
      secondQuest.Hero.ShouldBeSameAs(hero);
    }
  }


  public class SimpleService
  {
  }

  public class Hero
  {
    public string Name { get; }

    public Hero(string name)
    {
      Name = name;
    }
  }

  public class Quest
  {
    public Hero Hero { get; }
    public Artifact Artifact { get; }

    public Quest(Hero hero, Artifact artifact)
    {
      Hero = hero;
      Artifact = artifact;
    }
  }
}