using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using OpenRasta.Configuration.MetaModel;

namespace Tests.DI.SpeedyGonzales
{
  public class TransientNode : GraphNode
  {
    public TransientNode(DependencyFactoryModel model, LambdaExpression factoryExpression) : base(model,
      factoryExpression)
    {
    }
  }

  public class SingletonNode : GraphNode
  {
    public SingletonNode(DependencyFactoryModel model, int registrationIndex, TypeBuilder tb,
      LambdaExpression factoryExpression)
      : base(model, factoryExpression)
    {
      Field = tb.DefineField($"_{model.ServiceType.Name}{model.ConcreteType.Name}{registrationIndex}",
        model.ServiceType, FieldAttributes.Private | FieldAttributes.InitOnly);
    }

    public FieldBuilder Field { get; set; }
  }

  public class GraphNode
  {
    public GraphNode(DependencyFactoryModel model, LambdaExpression factoryExpression)
    {
      Model = model;
      FactoryExpression = factoryExpression;
    }

    public Func<object> Factory { get; set; }

    public DependencyFactoryModel Model { get; }

    public List<GraphNode> Inputs { get; set; }

    public List<GraphNode> Dependents { get; set; }

    public LambdaExpression FactoryExpression { get; }
  }
}