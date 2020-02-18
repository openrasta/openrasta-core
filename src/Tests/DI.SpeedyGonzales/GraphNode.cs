using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using OpenRasta.Configuration.MetaModel;

namespace Tests.DI.SpeedyGonzales
{
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
    
    public LambdaExpression FactoryExpression { get;  }

    public void CompileFactory()
    {
      if (FactoryExpression.Parameters.Any() == false)
        Factory = (Func<object>) FactoryExpression.Compile();
      
    }
  }
}