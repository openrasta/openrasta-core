using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace OpenRasta.Graph
{
    public static class ResourceConfigurationExtensions{
      public static ResourceDefinition<T> Resource<T>(this ResourceConfiguration configuration) {
        return new ResourceDefinition<T>();
      }   
    }

  public class ResourceDefinition<T> {
    public UriDefinition<T> Uri(Expression<Func<TemplateBuilder<T>, TemplateBuilder<T>>> uri) {
      return new UriDefinition<T>(uri);
    } 
  }

  public class UriDefinition<T> {
    private readonly Expression<Func<TemplateBuilder<T>, TemplateBuilder<T>>> _uriBuilder;

    public UriDefinition(Expression<Func<TemplateBuilder<T>, TemplateBuilder<T>>> uriBuilder) {
      _uriBuilder = uriBuilder;
    }

    public string CreateUri(T instance) {
      return new UriVisitor<T>(_uriBuilder).ToUri(instance);
    }
  }

  public abstract class Segment {
    public abstract StringBuilder Write(StringBuilder sb);

  }

  public class StringSegment : Segment{
    private readonly string _text;

    public StringSegment(string text) {
      _text = text;
    }

    public override StringBuilder Write(StringBuilder sb) {
      return sb.Append(_text).Append("/");
    }
  }
  public class TemplateBuilder<T> {
    public T Resource { get; }


    readonly List<Segment> _segments = new List<Segment>();

    public static TemplateBuilder<T> operator /(TemplateBuilder<T> left, object right) {
      return left;
    }
  }

  public sealed class UriVisitor<T> : ExpressionVisitor {
    private readonly Expression<Func<TemplateBuilder<T>, TemplateBuilder<T>>> _tree;
    private Segment _currentSegment;
    private IList<Segment> _segments = new List<Segment>();
    public UriVisitor(Expression<Func<TemplateBuilder<T>, TemplateBuilder<T>>> tree) {
      _tree = tree;
      Visit(_tree);
    }
    protected override Expression VisitBinary(BinaryExpression node) {
      if (node.NodeType != ExpressionType.Divide) return base.VisitBinary(node);
      CommitSegment(node.Left);
      CommitSegment(node.Right);
      return node;
    }

    private void CommitSegment(Expression expression) {
      Visit(expression);
      if (_currentSegment != null) _segments.Add(_currentSegment);
      _currentSegment = null;
    }

    protected override Expression VisitConstant(ConstantExpression node) {
      Segment segment = null;
      if (node.Type != typeof(string)) return base.VisitConstant(node);
      _currentSegment = new StringSegment((string)node.Value);
      return node;
    }

    public string ToUri(T instance) {
      return _segments.Aggregate(new StringBuilder("/"), (sb, segment) => segment.Write(sb)).ToString();
    }
  }
}