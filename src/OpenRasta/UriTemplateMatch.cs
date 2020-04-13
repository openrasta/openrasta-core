using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace OpenRasta
{
  public class UriTemplateMatch
  {
    public Uri BaseUri { get; set; }
    
    /// <summary>
    /// A collection of all path segment variables. /{value}/ => [value: xxx]
    /// </summary>
    public NameValueCollection PathSegmentVariables { get; internal set; }
    public object Data { get; set; }
    /// <summary>
    /// A collection of all query string parameters. ?key=value would contain [key]
    /// </summary>
    public Collection<string> QueryParameters { get; set; }
    /// <summary>
    /// A collection of all query string variables. ?key={id} contains [id=input]
    /// </summary>
    public NameValueCollection QueryStringVariables { get; internal set; }
    
    /// <summary>
    /// A collection of each segment in the matched URI. /test/value: [test,value]
    /// </summary>
    public IReadOnlyCollection<string> RelativePathSegments { get; internal set; }
    public Uri RequestUri { get; set; }
    public UriTemplate Template { get; set; }
    /// <summary>
    /// All segments matched by the wildcard variable
    /// </summary>
    public Collection<string> WildcardPathSegments { get; internal set; }
    public IEnumerable<UriTemplate.QuerySegment> QueryString { get; set; }
    public int Score { get; set; }
  }
}