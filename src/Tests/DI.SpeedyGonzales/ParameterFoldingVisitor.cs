using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Tests.DI.SpeedyGonzales
{
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
}