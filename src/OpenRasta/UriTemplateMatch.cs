using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace OpenRasta
{
  public class UriTemplateMatch
  {
    public Uri BaseUri { get; set; }
    public NameValueCollection PathSegmentVariables { get; internal set; }
    public object Data { get; set; }
    public Collection<string> QueryParameters { get; set; }
    public NameValueCollection QueryStringVariables { get; internal set; }
    public IReadOnlyCollection<string> RelativePathSegments { get; internal set; }
    public Uri RequestUri { get; set; }
    public UriTemplate Template { get; set; }
    public Collection<string> WildcardPathSegments { get; internal set; }
    public IEnumerable<UriTemplate.QuerySegment> QueryString { get; set; }
  }
}