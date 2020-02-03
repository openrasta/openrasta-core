using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenRasta.Configuration.Fluent.Implementation
{
  public class UriExpressionVisitor : MethodArgumentVisitor
  {
    Type _resourceType;

    public ParsedUri GenerateUri(Type resourceType, Expression uri)
    {
      _resourceType = resourceType;
      var expression = Visit(uri);
      var firstArg = (string) _stringFormatArgs.First();
      var uriTemplate = _stringFormatArgs.Count == 1
        ? firstArg
        : string.Format(firstArg, _stringFormatArgs.Skip(1).ToArray());
      
      return new ParsedUri(
        uriTemplate,
        expression);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
      var visitedExpression = base.VisitMember(node);

      if (node.NodeType != ExpressionType.MemberAccess) return visitedExpression;

      switch (node.Expression)
      {
        case ConstantExpression fieldTarget when node.Member is FieldInfo field &&
                                                 field.DeclaringType.IsAssignableFrom(_resourceType) == false:
        {
          var fieldValue = field.GetValue(fieldTarget.Value);
          return Expression.Constant(fieldValue);
        }
        case ConstantExpression propTarget
          when node.Member is PropertyInfo p && p.DeclaringType.IsAssignableFrom(_resourceType) == false:
        {
          var fieldValue = p.GetValue(propTarget.Value);
          return Expression.Constant(fieldValue);
        }
        default:
        {
          _memberAccess.Add(node.Member);
          break;
        }
      }

      return visitedExpression;
    }

    List<object> _stringFormatArgs;
    List<MemberInfo> _memberAccess;

    protected override Expression VisitArgument(int position, Expression arg)
    {
      if (position == 1 && arg is NewArrayExpression newArray && arg.Type == typeof(object[]))
      {
        Expression[] visitedValues = new Expression[newArray.Expressions.Count];
        bool rewrite = false;
        // special case string.Format(mystr,myarr)
        for (int i = 0; i < newArray.Expressions.Count; i++)
        {
          var toVisit = newArray.Expressions[i];
          var visited = VisitArgument(position + i, toVisit);
          visitedValues[i] = visited;
          rewrite = rewrite || visited != toVisit;
        }

        return rewrite ? Expression.NewArrayInit(typeof(object), visitedValues) : newArray;
      }

      _memberAccess = new List<MemberInfo>();
      var arg2 = base.VisitArgument(position, arg);

      if (arg2 is ConstantExpression constVal)
      {
        _stringFormatArgs.Add(constVal.Value);
        return arg2;
      }

      var memberAccessRootType = _memberAccess.FirstOrDefault()?.DeclaringType;

      if (memberAccessRootType?.IsAssignableFrom(_resourceType) == true)
      {
        // at the root
        _stringFormatArgs.Add($"{{{string.Concat(_memberAccess.Select(p => p.Name))}}}");
        _memberAccess.Clear();
        return arg2;
      }

      throw new InvalidOperationException($"Unrecognized expression {arg2}");
    }


    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
      if (node.Method.DeclaringType != typeof(string) || node.Method.Name != nameof(string.Format))
        throw new InvalidOperationException($"Method {node.Method} unsupported");

      _stringFormatArgs = new List<object>();

      return base.VisitMethodCall(node);
    }
  }
}