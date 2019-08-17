using System;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.ExpressionTree
{
  public static class ExpressionExtensions
  {
    public static Expression Assign<T>(this Variable<T> var, T value)
    {
      return Expression.Assign(var, Expression.Constant(value));
    }
    public static MethodCall<TReturn> Call<T, TReturn>(this TypedExpression<T> target, Expression<Func<T, TReturn>> method)
    {
      return new MethodCall<TReturn>(Expression.Call(target, MethodInfoVisitor.GetMethod(method)));
    }

    public static MemberAccess<TReturn> get_<T, TReturn>(this TypedExpression<T> target,
      Expression<Func<T, TReturn>> getter)
    {
      var visitor = MemberAccessVisitor.GetProperty(getter);
      return new MemberAccess<TReturn>(Expression.MakeMemberAccess(target, visitor));
    }

    class MemberAccessVisitor : ExpressionVisitor
    {
      public static PropertyInfo GetProperty(Expression expression)
      {
        var visitor = new MemberAccessVisitor();
        visitor.Visit(expression);
        return visitor._pi;
      }

      PropertyInfo _pi;

      protected override Expression VisitMember(MemberExpression node)
      {
        if (_pi != null) throw new InvalidOperationException("Cannot use lambda for multiple prop access");
        _pi = (PropertyInfo) node.Member;
        return base.VisitMember(node);
      }
    }

    class MethodInfoVisitor : ExpressionVisitor
    {
      MethodInfo _mi;

      public static MethodInfo GetMethod(Expression expression)
      {
        var visitor = new MethodInfoVisitor();
        visitor.Visit(expression);
        return visitor._mi;
      }

      protected override Expression VisitMethodCall(MethodCallExpression node)
      {
        if (_mi != null) throw new InvalidOperationException("Method already found");
        _mi = node.Method;
        return base.VisitMethodCall(node);
      }
    }
  }
}