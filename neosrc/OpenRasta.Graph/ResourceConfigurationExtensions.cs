using System;
using System.Collections.Generic;

namespace OpenRasta.Graph
{
    public static class ResourceConfigurationExtensions{
      public static ResourceDefinition<T> Resource<T>(this ResourceConfiguration configuration) {
        return new ResourceDefinition<T>();
      }   
    }

  public class ResourceDefinition<T> {
    public UriDefinition<T> Uri(Func<TemplateBuilder<T>, TemplateBuilder<T>> uri) {
      
      return new UriDefinition<T>(uri);
    } 
  }

  public class UriDefinition<T> {
    public UriDefinition(Func<TemplateBuilder<T>, TemplateBuilder<T>> uriBuilder) {
      
    }

  }

  public abstract class Segment {
    
  }

  public class StringSegment : Segment{
    private readonly string _text;

    public StringSegment(string text) {
      _text = text;
    }
  }
  public class TemplateBuilder<T> {
    public T Resource { get; }


    readonly List<Segment> _segments = new List<Segment>();

    public static TemplateBuilder<T> operator /(TemplateBuilder<T> left, string right) {
      left._segments.Add(new StringSegment(right));
      return left;
    }
    
  }
}