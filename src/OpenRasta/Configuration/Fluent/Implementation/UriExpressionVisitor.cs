using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenRasta.Configuration.Fluent.Implementation
{
  public class UriExpressionVisitor : ExpressionVisitor
  {
    readonly List<string> format = new List<string>();
    Type _resourceType;
    string _formatted;

    public string GenerateUri(Type resourceType, Expression uri)
    {
      _resourceType = resourceType;
      Visit(uri);
      return _formatted ?? format[0];
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
      return base.VisitParameter(node);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
      if (node.NodeType == ExpressionType.MemberAccess &&
          node.Member is PropertyInfo property &&
          property.DeclaringType.IsAssignableFrom(_resourceType))
      {
        format.Add($"{{{node.Member.Name}}}");
      }
      else if (node.NodeType == ExpressionType.MemberAccess &&
               node.Expression is ConstantExpression fieldTarget &&
               node.Member is FieldInfo field &&
               field.DeclaringType.IsAssignableFrom(_resourceType) == false)
      {
        var fieldValue = field.GetValue(fieldTarget.Value);
        format.Add((string)fieldValue);
      }
      else if (node.NodeType == ExpressionType.MemberAccess &&
               node.Expression is ConstantExpression propTarget &&
               node.Member is PropertyInfo p &&
               p.DeclaringType.IsAssignableFrom(_resourceType) == false)
      {
        var fieldValue = p.GetValue(propTarget.Value);
        format.Add((string)fieldValue);
      }
      return base.VisitMember(node);
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
      if (node.Method.DeclaringType == typeof(string) && node.Method.Name == nameof(string.Format))
      {
        var e = base.VisitMethodCall(node);
        _formatted = string.Format(format[0], format.Skip(1).Cast<object>().ToArray());
        return e;
      }

      return base.VisitMethodCall(node);
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
      if (node.Type == typeof(string))
        format.Add((string)node.Value);
      return base.VisitConstant(node);
    }
  }
}