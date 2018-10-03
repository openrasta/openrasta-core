using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using OpenRasta.Collections;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.TypeSystem;

namespace OpenRasta.Web
{
  public class TemplatedUriResolver : IUriResolver, IUriTemplateParser
  {
    UriTemplateTable _templates = new UriTemplateTable();
    public ITypeSystem TypeSystem { get; set; }
    public int Count => _templates.KeyValuePairs.Count;
    public IDictionary<object, HashSet<string>> UriNames { get; } = new Dictionary<object, HashSet<string>>();

    public bool IsReadOnly => _templates.IsReadOnly;

    public TemplatedUriResolver()
    {
      TypeSystem = TypeSystems.Default;
    }
  
    /// <exception cref="InvalidOperationException">Cannot add a Uri mapping once the configuration has been done.</exception>
    /// <exception cref="ArgumentException">Cannot use a Type as the resourceKey. NotifyAsync an <see cref="IType"/> instead or assign the <see cref="TypeSystem"/> property.</exception>
    public void Add(UriRegistration registration)
    {
      if (_templates.IsReadOnly)
        throw new InvalidOperationException("Cannot add a Uri mapping once the configuration has been done.");
      var resourceKey = EnsureIsNotType(registration.ResourceKey);
      var descriptor = new UrlDescriptor
      {
          Uri = new UriTemplate(registration.UriTemplate),

          Culture = registration.UriCulture,
          ResourceKey = resourceKey,
          UriName = registration.UriName,
          Registration = registration
      };
      _templates.KeyValuePairs.Add(new KeyValuePair<UriTemplate, object>(descriptor.Uri, descriptor));
      _templates.BaseAddress = new Uri("http://localhost/").IgnoreAuthority();
      var keys = UriNamesForKey(resourceKey);

      if (registration.UriName != null)
      {
        keys.Add(registration.UriName);
      }
    }

    HashSet<string> UriNamesForKey(object resourceKey)
    {
      if (UriNames.TryGetValue(resourceKey, out var names) == false)
        UriNames[resourceKey] = names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      return names;
    }

    public void Clear()
    {
      _templates = new UriTemplateTable();
      UriNames.Clear();
    }

    public bool Contains(UriRegistration item)
    {
      return this.Any(x => x == item);
    }

    public void CopyTo(UriRegistration[] array, int arrayIndex)
    {
      this.ToList().CopyTo(array, arrayIndex);
    }

    public bool Remove(UriRegistration registration)
    {
      var pairToRemove = _templates.KeyValuePairs
          .Where(x => ((UrlDescriptor)x.Value).Registration == registration)
          .ToList();

      if (pairToRemove.Count <= 0) return false;

      _templates.KeyValuePairs.Remove(pairToRemove[0]);
      if (registration.UriName != null)
        UriNames[registration.ResourceKey].Remove(registration.UriName);
      return true;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<UriRegistration> GetEnumerator()
    {
      return _templates.KeyValuePairs.Select(x => ((UrlDescriptor)x.Value).Registration).GetEnumerator();
    }

    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    public Uri CreateUriFor(Uri baseAddress, object resourceKey, string uriName, NameValueCollection keyValues)
    {
      resourceKey = EnsureIsNotType(resourceKey);
      var template = FindBestMatchingTemplate(_templates, resourceKey, uriName, keyValues);

      if (template == null)
      {
        throw new InvalidOperationException(
            $"No suitable Uri could be found for resource with key {resourceKey} with values {keyValues.ToHtmlFormEncoding()}.");
      }

      return template.BindByName(baseAddress, keyValues);
    }

    public UriRegistration Match(Uri uriToMatch)
    {
      if (uriToMatch == null)
        return null;
      var tableMatches = _templates.Match(uriToMatch.IgnoreSchemePortAndAuthority());
      if (tableMatches == null || tableMatches.Count == 0)
        return null;
      var urlDescriptor = (UrlDescriptor)tableMatches[0].Data;


      var allResults = tableMatches.Select(m =>
      {
        var descriptor = (UrlDescriptor)m.Data;
        return new TemplatedUriMatch(
            descriptor.Registration.ResourceModel,
            descriptor.Registration.UriModel,
            m);
      }).ToList();

      return new UriRegistration(
          urlDescriptor.Registration.ResourceModel,
          urlDescriptor.Registration.UriModel)
      {
          Results = allResults
      };
    }

    public IEnumerable<string> GetQueryParameterNamesFor(string uriTemplate)
    {
      return new UriTemplate(uriTemplate).QueryStringVariableNames;
    }

    public IEnumerable<string> GetTemplateParameterNamesFor(string uriTemplate)
    {
      return new UriTemplate(uriTemplate).PathSegmentVariableNames;
    }

    static bool CompatibleKeys(object requestResourceKey, object templateResourceKey)
    {
      var requestType = requestResourceKey as IType;
      var templateType = templateResourceKey as IType;
      return (requestType != null &&
              templateType != null &&
              requestType.IsAssignableTo(templateType)) ||
             requestResourceKey.Equals(templateResourceKey);
    }

    object EnsureIsNotType(object resourceKey)
    {
      var resourceType = resourceKey as Type;
      if (resourceType != null)
        resourceKey = TypeSystem.FromClr(resourceType);
      return resourceKey;
    }

    UriTemplate FindBestMatchingTemplate(UriTemplateTable templates,
        object resourceKey,
        string uriName,
        NameValueCollection keyValues)
    {
      resourceKey = EnsureIsNotType(resourceKey);
      var matchingTemplates =
          from template in templates.KeyValuePairs
          let descriptor = (UrlDescriptor)template.Value
          where CompatibleKeys(resourceKey, descriptor.ResourceKey)
          where UriNameMatches(uriName, descriptor.UriName)
          let templateParameters =
              template.Key.PathSegmentVariableNames
                .Concat(template.Key.QueryStringVariableNames)
                .Concat(template.Key.FragmentVariableNames).ToList()
          let hasKeys = keyValues != null && keyValues.HasKeys()
          where (templateParameters.Count == 0) ||
                (templateParameters.Count > 0
                 && hasKeys
                 && templateParameters.All(x => keyValues.AllKeys.Contains(x, StringComparison.OrdinalIgnoreCase)))
          orderby templateParameters.Count descending
          select template.Key;

      return matchingTemplates.FirstOrDefault();
    }

    static bool UriNameMatches(string requestUriName, string templateUriName)
    {
      return (!requestUriName.IsNullOrEmpty() &&
              requestUriName.EqualsOrdinalIgnoreCase(templateUriName)) ||
             (requestUriName.IsNullOrEmpty() &&
              templateUriName.IsNullOrEmpty());
    }

    class UrlDescriptor
    {
      public CultureInfo Culture { get; set; }
      public UriRegistration Registration { get; set; }
      public object ResourceKey { get; set; }
      public UriTemplate Uri { get; set; }
      public string UriName { get; set; }
    }
  }

  public class TemplatedUriMatch
  {
    public ResourceModel ResourceModel { get; }
    public UriModel UriModel { get; }
    public UriTemplateMatch Match { get; }

    public TemplatedUriMatch(ResourceModel resourceModel, UriModel uriModel, UriTemplateMatch match)
    {
      ResourceModel = resourceModel;
      UriModel = uriModel;
      Match = match;
    }
  }
}