using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace OpenRasta
{
  public class UriTemplate
  {
    const string WILDCARD_TEXT = "*";
    readonly Dictionary<string, UrlSegment> _pathSegmentVariables;
    readonly List<UrlSegment> _segments;
    readonly Dictionary<string, QuerySegment> _queryStringSegments;
    readonly Uri _templateUri;

    public UriTemplate(string template)
    {
      _templateUri = ParseTemplate(template);
      _segments = ParsePathSegments(_templateUri);

      _pathSegmentVariables = _segments.Where(segment => segment.Type == SegmentType.Variable)
        .ToDictionary(segment => segment.Text.ToUpperInvariant(), StringComparer.OrdinalIgnoreCase);

      QueryString = ParseQueryStringSegments(_templateUri.Query).ToList();
      Fragment = ParseFragment(_templateUri.Fragment).ToList();
      _queryStringSegments = ParseQueryStringSegments(QueryString);

      PathSegmentVariableNames = new ReadOnlyCollection<string>(new List<string>(_pathSegmentVariables.Keys));
      QueryStringVariableNames =
        new ReadOnlyCollection<string>(new List<string>(GetQueryStringVariableNames(_queryStringSegments)));
      FragmentVariableNames =
        new ReadOnlyCollection<string>(Fragment.Where(f => f.Type == SegmentType.Variable).Select(f => f.Text)
          .ToList());
    }

    public List<FragmentSegment> Fragment { get; }
    const string LBRACE = "%7B";
    const string RBRACE = "%7D";

    static IEnumerable<FragmentSegment> ParseFragment(string templateUriFragment)
    {
      if (templateUriFragment.Length == 0) yield break;
      int openBraceFragmentPos = templateUriFragment.IndexOf(LBRACE, StringComparison.OrdinalIgnoreCase);
      if (openBraceFragmentPos == -1)
      {
        yield return new FragmentSegment {Text = templateUriFragment, Type = SegmentType.Literal};
        yield break;
      }

      var pos = 0;
      do
      {
        if (openBraceFragmentPos > pos)
        {
          yield return new FragmentSegment
          {
            Text = templateUriFragment.Substring(pos, openBraceFragmentPos - pos),
            Type = SegmentType.Literal
          };
          pos = openBraceFragmentPos;
        }

        var endBracePos =
          templateUriFragment.IndexOf(RBRACE, openBraceFragmentPos + 1, StringComparison.OrdinalIgnoreCase);
        if (endBracePos == -1)
        {
          yield return new FragmentSegment {Text = templateUriFragment.Substring(openBraceFragmentPos)};
          yield break;
        }

        var varNameLength = endBracePos - openBraceFragmentPos - LBRACE.Length;
        if (varNameLength == 0)
        {
          yield return new FragmentSegment {Text = LBRACE + RBRACE, Type = SegmentType.Literal};
          continue;
        }

        yield return new FragmentSegment
        {
          Type = SegmentType.Variable,
          Text = templateUriFragment.Substring(openBraceFragmentPos + LBRACE.Length, varNameLength)
        };
        pos = endBracePos + RBRACE.Length;
      } while ((openBraceFragmentPos =
                 templateUriFragment.IndexOf(LBRACE, openBraceFragmentPos + 1, StringComparison.OrdinalIgnoreCase)) !=
               -1);

      if (pos < templateUriFragment.Length)
        yield return new FragmentSegment
        {
          Text = templateUriFragment.Substring(pos),
          Type = SegmentType.Literal
        };
    }

    public IEnumerable<QuerySegment> QueryString { get; }

    public ReadOnlyCollection<string> PathSegmentVariableNames { get; }
    public ReadOnlyCollection<string> QueryStringVariableNames { get; }
    public ReadOnlyCollection<string> FragmentVariableNames { get; }

    IEnumerable<string> GetQueryStringVariableNames(Dictionary<string, QuerySegment> valueCollection)
    {
      return
        from qsegment in valueCollection
        where qsegment.Value.Type == SegmentType.Variable
        select qsegment.Value.Value;
    }

    static Dictionary<string, QuerySegment> ParseQueryStringSegments(IEnumerable<QuerySegment> queryString)
    {
      var result = new Dictionary<string, QuerySegment>(StringComparer.OrdinalIgnoreCase);
      foreach (var qs in queryString)
      {
        if (!result.ContainsKey(qs.Key))
        {
          result.Add(qs.Key, qs);
        }
      }

      return result;
    }

    struct QsParseResult
    {
      public StringSegment Key;
      public StringSegment Value;
    }

    enum ParseState
    {
      QsKey,
      QsValue,
      End,
      Begin
    }


    // Preserved for compatibility
    public static IEnumerable<QuerySegment> ParseQueryStringSegments(string query)
      => ParseQueryStringSegmentsInternal(new StringSegment(query, 0, query.Length), true);


    static LinkedList<QuerySegment> ParseQueryStringSegmentsInternal(in StringSegment query, bool isTemplate)
    {
      var nodeList = new LinkedList<QuerySegment>();

      var boundary = 0;
      var state = ParseState.Begin;

      QsParseResult currentKey = default;

      void append()
      {
        nodeList.AddLast(ToQueryStringSegment(currentKey, isTemplate));
        currentKey = new QsParseResult();
      }

      for (var pos = 0; pos < query.Count; pos++)
      {
        var c = query[pos];

        void commit(StringSegment q)
        {
          switch (state)
          {
            case ParseState.QsKey:
              currentKey.Key = q.Subsegment(boundary, pos - boundary);
              break;
            case ParseState.QsValue:
              currentKey.Value = q.Subsegment(boundary, pos - boundary);
              break;
          }

          boundary = pos + 1;
        }

        if (state == ParseState.Begin)
        {
          if (c == '?')
          {
            state = ParseState.QsKey;
            boundary = pos + 1;
          }
          // ignore it all till the ?
          continue;
        }

        if (pos == query.Count - 1)
        {
          pos++;
          commit(query);
          append();
          state = ParseState.End;
          break;
        }

        if (c == '=' && state == ParseState.QsKey)
        {
          commit(query);
          state = ParseState.QsValue;
          boundary = pos + 1;
        }
        else if (c == '&')
        {
          commit(query);
          append();
          state = ParseState.QsKey;
        }
      }

      return nodeList;
    }

    static QuerySegment ToQueryStringSegment(in QsParseResult entry, bool isTemplate)
    {
      string rawValue = null, value = null;
      var type = SegmentType.Literal;

      if (entry.Value.HasValue)
      {
        value = rawValue = Uri.UnescapeDataString(entry.Value.Value.Replace('+', ' '));

        if (isTemplate && rawValue[0] == '{' && rawValue[rawValue.Length - 1] == '}')
        {
          type = SegmentType.Variable;
          value = rawValue.Substring(1, rawValue.Length - 2);
        }
      }
      else if (entry.Value.Buffer != null && entry.Value.Count == 0)
      {
        value = string.Empty;
        rawValue = string.Empty;
      }

      return new QuerySegment
      {
        Key = entry.Key.Value,
        Value = value,
        RawValue = rawValue,
        Type = type
      };
    }

    static List<UrlSegment> ParsePathSegments(Uri templateUri)
    {
      var passedSegments = new List<UrlSegment>();
      var originalSegments = templateUri.Segments;
      foreach (var segmentText in originalSegments)
      {
        UrlSegment parsedSegment;
        var unescapedSegment = Uri.UnescapeDataString(segmentText);
        //  TODO: Check we don't double decode the wrong / here, potential issue
        var sanitizedSegment = unescapedSegment.Replace("/", string.Empty);
        var trailingSeparator = unescapedSegment.Length - sanitizedSegment.Length > 0;

        string variableName;
        if (sanitizedSegment == string.Empty) // this is the '/' returned by Uri which we don't care much for
          continue;
        if ((variableName = GetVariableName(unescapedSegment)) != null)
          parsedSegment = new UrlSegment
          {
            Text = variableName, OriginalText = sanitizedSegment, Type = SegmentType.Variable,
            TrailingSeparator = trailingSeparator
          };
        else if (string.Compare(unescapedSegment, WILDCARD_TEXT, StringComparison.OrdinalIgnoreCase) == 0)
          parsedSegment = new UrlSegment
            {Text = WILDCARD_TEXT, OriginalText = sanitizedSegment, Type = SegmentType.Wildcard};
        else
          parsedSegment = new UrlSegment
          {
            Text = sanitizedSegment, OriginalText = sanitizedSegment, Type = SegmentType.Literal,
            TrailingSeparator = trailingSeparator
          };

        passedSegments.Add(parsedSegment);
      }

      return passedSegments;
    }

    static string GetVariableName(string segmentText)
    {
      segmentText = segmentText.Replace("/", string.Empty).Trim();

      string result = null;
      if (segmentText.Length > 2 && segmentText[0] == '{' && segmentText[segmentText.Length - 1] == '}')
        result = segmentText.Substring(1, segmentText.Length - 2);

      return result;
    }

    static Uri ParseTemplate(string template)
    {
      return new Uri(new Uri("http://uritemplateimpl"), template);
    }

    public Uri BindByName(Uri baseAddress, NameValueCollection parameters)
    {
      if (baseAddress == null)
        throw new ArgumentNullException(nameof(baseAddress),
          "The base Uri needs to be provided for a Uri to be generated.");


      baseAddress = SanitizeUriAsBaseUri(baseAddress);

      var path = new StringBuilder();

      foreach (var segment in _segments)
      {
        if (segment.Type == SegmentType.Literal)
          path.Append(segment.Text);
        else if (segment.Type == SegmentType.Variable)
        {
          var value = parameters[segment.Text.ToUpperInvariant()];


          path
            .Append(value.Replace("/", "%2F")
              .Replace("?", "%3F")
              .Replace("#", "%23"));
        }

        if (segment.TrailingSeparator)
          path.Append('/');
      }

      if (_queryStringSegments.Count > 0)
      {
        path.Append('?');
        foreach (var querySegment in _queryStringSegments)
        {
          var qsValue = parameters[querySegment.Value.Value]
            .Replace("&", "%25")
            .Replace("#", "%23");

          path.Append(querySegment.Value.Key).Append("=")
            .Append(qsValue).Append("&");
        }

        path.Remove(path.Length - 1, 1);
      }

      foreach (var frag in Fragment)
      {
        if (frag.Type == SegmentType.Literal)
          path.Append(frag.Text);
        else if (frag.Type == SegmentType.Variable)
          path.Append(parameters[frag.Text.ToUpperInvariant()]);
      }

      return new Uri(baseAddress, path.ToString());
    }

    static Uri SanitizeUriAsBaseUri(Uri address)
    {
      var builder = new UriBuilder(address)
      {
        Fragment = string.Empty,
        Query = string.Empty
      };
      if (!builder.Path.EndsWith("/"))
        builder.Path += "/";
      return builder.Uri;
    }

    public Uri BindByPosition(Uri baseAddress, params string[] values)
    {
      baseAddress = SanitizeUriAsBaseUri(baseAddress);
      var path = new StringBuilder();
      var paramPosition = 0;
      foreach (var segment in _segments)
      {
        switch (segment.Type)
        {
          case SegmentType.Literal:
            path.Append(segment.Text);
            break;
          case SegmentType.Variable:
            var param = paramPosition < values.Length ? values[paramPosition++] : segment.Text;
            path.Append(param);
            break;
        }

        if (segment.TrailingSeparator)
          path.Append('/');
      }

      return new Uri(baseAddress, path.ToString());
    }

    public bool IsEquivalentTo(UriTemplate other)
    {
      if (_segments.Count != other?._segments.Count)
        return false;
      if (_queryStringSegments.Count != other._queryStringSegments.Count)
        return false;
      for (var i = 0; i < _segments.Count; i++)
      {
        var thisSegment = _segments[i];
        var otherSegment = other._segments[i];
        if (thisSegment.Type != otherSegment.Type)
          return false;
        if (thisSegment.Type == SegmentType.Literal && thisSegment.Text != otherSegment.Text)
          return false;
      }

      foreach (var thisSegment in _queryStringSegments)
      {
        if (!other._queryStringSegments.ContainsKey(thisSegment.Key))
          return false;
        var otherSegment = other._queryStringSegments[thisSegment.Key];

        if (thisSegment.Value.Type != otherSegment.Type)
          return false;
        if (thisSegment.Value.Type == SegmentType.Literal && thisSegment.Value.Value != otherSegment.Value)
          return false;
      }

      return true;
    }

    static StringSegment PathAndQuery(in Uri uri)
    {
      var str = uri.ToString();
      var idx = str.IndexOf('/', 8); // covers http://x and https://
      if (idx == -1) idx = str.Length - 1;
      return new StringSegment(str, idx, str.Length - idx);
    }

    public UriTemplateMatch Match(Uri baseAddress, Uri uri)
    {
      if (baseAddress == null || uri == null)
        return null;

      if (baseAddress == null) throw new ArgumentNullException(nameof(baseAddress));

      if (ReferenceEquals(baseAddress, UriTemplateTable.DefaultBaseAddress) == false &&
          baseAddress.GetLeftPart(UriPartial.Authority) != uri.GetLeftPart(UriPartial.Authority))
        return null;

      var baseUriPathAndQuery = PathAndQuery(baseAddress);
      var requestUriPathAndQuery = PathAndQuery(uri);

      var baseUriSegments = ParsePathSegments(baseUriPathAndQuery);
      var candidateSegments = ParsePathSegments(requestUriPathAndQuery);

      var currentBaseUriSegment = baseUriSegments.First;

      while (true)
      {
        if (currentBaseUriSegment != null && candidateSegments.First != null &&
            currentBaseUriSegment.Value.Equals(candidateSegments.First.Value, StringComparison.Ordinal))
        {
          candidateSegments.RemoveFirst();
          currentBaseUriSegment = currentBaseUriSegment.Next;
        }
        else
          break;
      }

      if (candidateSegments.Count != _segments.Count)
        return null;

      var boundVariables = new NameValueCollection(_pathSegmentVariables.Count);

      var firstCandidateSegment = candidateSegments.First;
      var currentCandidateSegment = firstCandidateSegment;

      for (var i = 0; i < _segments.Count; i++)
      {
        if (currentCandidateSegment == null) break;
        var proposedSegment = _segments[i];

        var proposedSegmentText = proposedSegment.Text;

        string unescapeSegment() => Uri.UnescapeDataString(currentCandidateSegment.Value.ToString());

        switch (proposedSegment.Type)
        {
          case SegmentType.Wildcard:
            throw new NotImplementedException("Not finished wildcards implementation yet");
          case SegmentType.Variable:
            boundVariables.Add(proposedSegmentText, unescapeSegment());
            break;
          case SegmentType.Literal when
            string.Equals(proposedSegmentText, unescapeSegment(), StringComparison.OrdinalIgnoreCase) == false:
            return null;
        }

        currentCandidateSegment = currentCandidateSegment.Next;
      }

      var queryStringVariables = new NameValueCollection();
      var uriQuery = ParseQueryStringSegmentsInternal(requestUriPathAndQuery, false);
      var requestUriQuerySegments = ParseQueryStringSegments(uriQuery);

      var queryParams = new Collection<string>();

      foreach (var templateQuerySegment in QueryString)
      {
        var requestUriHasQueryStringKey = requestUriQuerySegments.ContainsKey(templateQuerySegment.Key);

        switch (templateQuerySegment.Type)
        {
          case SegmentType.Literal when requestUriHasQueryStringKey == false ||
                                        QuerySegmentValueIsDifferent(requestUriQuerySegments, templateQuerySegment):
            return null;
          case SegmentType.Literal:
            break;
          case SegmentType.Variable when requestUriHasQueryStringKey:
            queryStringVariables[templateQuerySegment.Value] =
              requestUriQuerySegments[templateQuerySegment.Key].RawValue;
            break;
        }

        queryParams.Add(templateQuerySegment.Key);
      }

      return new UriTemplateMatch
      {
        BaseUri = baseAddress,
        Data = 0,
        PathSegmentVariables = boundVariables,
        QueryString = uriQuery,
        QueryParameters = queryParams,
        QueryStringVariables = queryStringVariables,
        RelativePathSegments = From(candidateSegments, segment => segment.Value),
        RequestUri = uri,
        Template = this,
        WildcardPathSegments = new Collection<string>()
      };
    }

    public static IReadOnlyCollection<TValue> From<T, TValue>(
      LinkedList<T> nodes,
      Func<T, TValue> select)
    {
      return new LinkedListAdapter<T, TValue>(nodes, select);
    }

    public class LinkedListAdapter<T, TValue> : IReadOnlyCollection<TValue>
    {
      public int Count => _node.Count;

      readonly LinkedList<T> _node;
      readonly Func<T, TValue> _select;

      public LinkedListAdapter(LinkedList<T> node, Func<T, TValue> select)
      {
        _node = node;
        _select = select;
      }

      public IEnumerator<TValue> GetEnumerator()
      {
        return _node.Select(arg => _select(arg)).GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }
    }

    static bool QuerySegmentValueIsDifferent(
      Dictionary<string, QuerySegment> requestUriQuerySegments,
      QuerySegment templateQuerySegment)
    {
      return requestUriQuerySegments[templateQuerySegment.Key].Value != templateQuerySegment.Value;
    }

    static LinkedList<StringSegment> ParsePathSegments(string path) => ParsePathSegments(new StringSegment(path));

    static LinkedList<StringSegment> ParsePathSegments(in StringSegment path)
    {
      var boundary = 0;
      var segments = new LinkedList<StringSegment>();

      for (var pos = boundary; pos < path.Count; pos++)
      {
        if (path[pos] == '/')
        {
          segments.AddLast(path.Subsegment(boundary, pos - boundary));
          boundary = pos + 1;
        }
        else if (pos == path.Count - 1)
        {
          segments.AddLast(path.Subsegment(boundary, pos - boundary + 1));
          break;
        }
        else if (path[pos] == '?')
        {
          if (pos-boundary > 0)
            segments.AddLast(path.Subsegment(boundary, pos - boundary));
          break;
        }
      }

      return segments;
    }

    public override int GetHashCode()
    {
      var hash = 0;
      foreach (var segment in _segments)
      {
        hash ^= segment.OriginalText.GetHashCode();
      }

      return hash;
    }

    public override string ToString()
    {
      return Uri.UnescapeDataString(_templateUri.AbsolutePath);
    }

    public class QuerySegment
    {
      public string Key { get; set; }
      public string Value { get; set; }
      public SegmentType Type { get; set; }
      public string RawValue { get; set; }

      public override string ToString()
      {
        switch (Type)
        {
          case SegmentType.Wildcard:
            return "*";
          case SegmentType.Variable:
            return $"{Key}={{{Value}}}";
          case SegmentType.Literal:
            return Value == null ? Key : $"{Key}={Value}";
          default:
            throw new ArgumentOutOfRangeException();
        }
      }
    }

    public enum SegmentType
    {
      Wildcard,
      Variable,
      Literal
    }

    public class FragmentSegment
    {
      public SegmentType Type { get; set; }
      public string Text { get; set; }
    }

    class UrlSegment
    {
      public string OriginalText { get; set; }
      public string Text { get; set; }
      public SegmentType Type { get; set; }
      public bool TrailingSeparator { get; set; }
    }
  }


  readonly struct StringSegment : IEquatable<StringSegment>
  {
    readonly string _buffer;
    readonly int _offset;
    readonly int _count;

    // <summary>
    // Initializes a new instance of the <see cref="T:System.Object"/> class.
    // </summary>
    public StringSegment(string buffer, int offset, int count)
    {
      _buffer = buffer;
      _offset = offset;
      _count = count;
    }

    public StringSegment(string buffer)
    {
      _buffer = buffer;
      _offset = buffer == null ? -1 : 0;
      _count = buffer?.Length ?? 0;
    }

    public string Buffer => _buffer;

    public int Offset => _offset;

    public int Count => _count;

    public string Value => _offset == -1 ? null : _buffer.Substring(_offset, _count);

    public bool HasValue => _offset != -1 && _count != 0 && _buffer != null;

    public char this[in int index] => _buffer[_offset + index];

    public bool Equals(StringSegment other)
    {
      return string.Equals(_buffer, other._buffer) && _offset == other._offset && _count == other._count;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
      {
        return false;
      }

      return obj is StringSegment && Equals((StringSegment) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = (_buffer != null ? _buffer.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ _offset;
        hashCode = (hashCode * 397) ^ _count;
        return hashCode;
      }
    }

    public static bool operator ==(StringSegment left, StringSegment right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(StringSegment left, StringSegment right)
    {
      return !left.Equals(right);
    }

    public bool StartsWith(string text, StringComparison comparisonType)
    {
      if (text == null)
      {
        throw new ArgumentNullException("text");
      }

      var textLength = text.Length;
      if (!HasValue || _count < textLength)
      {
        return false;
      }

      return string.Compare(_buffer, _offset, text, 0, textLength, comparisonType) == 0;
    }

    public bool EndsWith(string text, StringComparison comparisonType)
    {
      if (text == null)
        throw new ArgumentNullException(nameof(text));

      var textLength = text.Length;
      if (!HasValue || _count < textLength)
      {
        return false;
      }

      return string.Compare(_buffer, _offset + _count - textLength, text, 0, textLength, comparisonType) == 0;
    }

    public bool Equals(StringSegment text, StringComparison comparisonType)
    {
      if (text == null)
        throw new ArgumentNullException(nameof(text));

      var textLength = text.Count;

      if (!HasValue && !text.HasValue) return true;
      if (_count != textLength)
      {
        return false;
      }

      if (_count == 0) return true;

      return string.Compare(_buffer, _offset, text._buffer, text._offset, textLength, comparisonType) == 0;
    }

    public bool Equals(string text, StringComparison comparisonType)
    {
      if (text == null)
      {
        throw new ArgumentNullException(nameof(text));
      }

      var textLength = text.Length;
      if (!HasValue || _count != textLength)
      {
        return false;
      }

      return string.Compare(_buffer, _offset, text, 0, textLength, comparisonType) == 0;
    }

    public string Substring(int offset, int length)
    {
      return _buffer.Substring(_offset + offset, length);
    }

    public StringSegment Subsegment(int offset, int length)
    {
      return new StringSegment(_buffer, _offset + offset, length);
    }

    public override string ToString()
    {
      return Value ?? string.Empty;
    }

    //
    // public enum UriSegmentType
    // {
    //   QsKey,
    //   QsValue,
    //   PathSegment
    // }
    // public static class UriParser
    // {
    //   public static LinkedList< 
    // }
  }
}