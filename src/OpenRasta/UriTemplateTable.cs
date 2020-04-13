using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenRasta.Collections;

namespace OpenRasta
{
  public class UriTemplateTable
  {
    public static readonly Uri DefaultBaseAddress = new Uri("http://localhost");
    readonly List<KeyValuePair<UriTemplate, object>> _keyValuePairs;
    ReadOnlyCollection<KeyValuePair<UriTemplate, object>> _keyValuePairsReadOnly;

    public UriTemplateTable(Uri baseAddress = null, IEnumerable<KeyValuePair<UriTemplate, object>> keyValuePairs = null)
    {
      BaseAddress = baseAddress == null 
        ? DefaultBaseAddress
        : baseAddress.Equals(DefaultBaseAddress) 
          ? DefaultBaseAddress 
          : baseAddress;
      
      _keyValuePairs = keyValuePairs != null
        ? new List<KeyValuePair<UriTemplate, object>>(keyValuePairs)
        : new List<KeyValuePair<UriTemplate, object>>();
    }

    public Uri BaseAddress { get; }

    public bool IsReadOnly { get; private set; }

    public IList<KeyValuePair<UriTemplate, object>> KeyValuePairs => IsReadOnly
      ? _keyValuePairsReadOnly
      : (IList<KeyValuePair<UriTemplate, object>>) _keyValuePairs;

    /// <exception cref="InvalidOperationException">You need to set a BaseAddress before calling MakeReadOnly</exception>
    public void MakeReadOnly(bool allowDuplicateEquivalentUriTemplates)
    {
      if (BaseAddress == null)
        throw new InvalidOperationException("You need to set a BaseAddress before calling MakeReadOnly");
      if (!allowDuplicateEquivalentUriTemplates)
        EnsureAllTemplatesAreDifferent();
      IsReadOnly = true;
      _keyValuePairsReadOnly = _keyValuePairs.AsReadOnly();
    }

    public Collection<UriTemplateMatch> Match(Uri uri)
    {
      var lastMaxLiteralSegmentCount = 0;
      var matches = new List<UriTemplateMatch>();
      foreach (var template in KeyValuePairs)
      {
        // TODO: discard uri templates with fragment identifiers until tests are implemented
        if (template.Key.Fragment.Any()) continue;
        var potentialMatch = template.Key.Match(BaseAddress, uri);

        if (potentialMatch == null) continue;

        // this calculates and keep only what matches the maximum possible amount of literal segments
        var currentMaxLiteralSegmentCount = potentialMatch.RelativePathSegments.Count
                                            - potentialMatch.WildcardPathSegments.Count;
        for (var i = 0; i < potentialMatch.PathSegmentVariables.Count; i++)
          if (potentialMatch.QueryParameters == null ||
              potentialMatch.QueryStringVariables[potentialMatch.PathSegmentVariables.GetKey(i)] == null)
            currentMaxLiteralSegmentCount -= 1;

        potentialMatch.Data = template.Value;

        if (currentMaxLiteralSegmentCount > lastMaxLiteralSegmentCount)
        {
          lastMaxLiteralSegmentCount = currentMaxLiteralSegmentCount;
        }
        else if (currentMaxLiteralSegmentCount < lastMaxLiteralSegmentCount)
        {
          continue;
        }
        var missingQueryStringParameters = Math.Abs(potentialMatch.QueryStringVariables.Count - potentialMatch.QueryParameters.Count);
        var matchedVariables = potentialMatch.PathSegmentVariables.Count + potentialMatch.QueryStringVariables.Count;
        var literalSegments = potentialMatch.RelativePathSegments.Count - potentialMatch.PathSegmentVariables.Count;
        potentialMatch.Score =
          literalSegments << 24
          | matchedVariables << 16
          | ((1 << 8) - missingQueryStringParameters);
        matches.Add(potentialMatch);
      }

      matches.Sort((left, right) => left.Score.CompareTo(right.Score)*-1);
      return new Collection<UriTemplateMatch>(matches);
    }

    /// <exception cref="UriTemplateMatchException">Several matching templates were found.</exception>
    public UriTemplateMatch MatchSingle(Uri uri)
    {
      UriTemplateMatch singleMatch = null;
      foreach (var segmentKey in KeyValuePairs)
      {
        UriTemplateMatch potentialMatch = segmentKey.Key.Match(BaseAddress, uri);
        if (potentialMatch != null && singleMatch != null)
          throw new UriTemplateMatchException("Several matching templates were found.");
        if (potentialMatch != null)
        {
          singleMatch = potentialMatch;
          singleMatch.Data = segmentKey.Value;
        }
      }

      return singleMatch;
    }

    /// <exception cref="InvalidOperationException">Two equivalent templates were found.</exception>
    void EnsureAllTemplatesAreDifferent()
    {
      for (int i = 0; i < _keyValuePairs.Count; i++)
      {
        var rootKey = _keyValuePairs[i];
        for (var j = i + 1; j < _keyValuePairs.Count; j++)
          if (rootKey.Key.IsEquivalentTo(_keyValuePairs[j].Key))
            throw new InvalidOperationException("Two equivalent templates were found.");
      }
    }
  }
}