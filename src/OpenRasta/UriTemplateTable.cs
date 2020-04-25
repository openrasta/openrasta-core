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
    readonly IEnumerable<KeyValuePair<UriTemplate, object>> _resolvablePairs;

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

      _resolvablePairs = _keyValuePairs.Where(pair => pair.Key.Fragment.Any() == false);
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

    public Collection<UriTemplateMatch> Match(Uri uri, Uri baseAddress = null)
    {
        var appBase = baseAddress ?? BaseAddress;
        
      var lastMaxLiteralSegmentCount = 0;
      
      var matches = new List<UriTemplateMatch>();
      foreach (var template in _resolvablePairs)
      {
        var potentialMatch = template.Key.Match(appBase, uri);

        if (potentialMatch == null) continue;

        
        // var pathSegments = potentialMatch.RelativePathSegments.Count;
        // WARNING, CODE THAT MAKES NO SENSE MAKES NO SENSE AT ALL!
        // What it used to say, and it's not matching the code:
        //
        // this calculates and keep only what matches the maximum possible amount of literal segments
        
        // how many of the path segments were actually matched by a var or a literal
        // that is all path segments less the wildcard ones
        // example: /first/second/third/fourth, with /{a}/{b}/{*} would be the first two segments
        var pathSegmentMatchedToLiteralsOrVars = potentialMatch.RelativePathSegments.Count
                                                      - potentialMatch.WildcardPathSegments.Count;
        
        
        var currentLiteralSegmentCount = pathSegmentMatchedToLiteralsOrVars;
        
        // foreach of the path segment vars, {a}=value
        for (var i = 0; i < potentialMatch.PathSegmentVariables.Count; i++)
        {
          // var name = a
          var pathSegmentVarName = potentialMatch.PathSegmentVariables.GetKey(i);
          // look for lack of query parameters OR ?something={a} missing
          if (potentialMatch.QueryParameters == null ||
              potentialMatch.QueryStringVariables[pathSegmentVarName] == null)
          {
            currentLiteralSegmentCount -= 1;
          }
        }

        
        if (currentLiteralSegmentCount > lastMaxLiteralSegmentCount)
        {
          lastMaxLiteralSegmentCount = currentLiteralSegmentCount;
        }
        else if (currentLiteralSegmentCount < lastMaxLiteralSegmentCount)
        {
          continue;
        }

        var missingQueryStringParameters =
          Math.Abs(potentialMatch.QueryStringVariables.Count - potentialMatch.QueryParameters.Count);
        var matchedVariables = potentialMatch.PathSegmentVariables.Count + potentialMatch.QueryStringVariables.Count;

        var literalSegments = pathSegmentMatchedToLiteralsOrVars
                              - potentialMatch.PathSegmentVariables.Count
                              - potentialMatch.WildcardPathSegments.Count;

        potentialMatch.Data = template.Value;
        potentialMatch.Score =
          literalSegments << 24
          | matchedVariables << 16
          | ((1 << 8) - missingQueryStringParameters);
        matches.Add(potentialMatch);
      }

      matches.Sort((left, right) => left.Score.CompareTo(right.Score) * -1);
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