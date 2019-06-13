#region License

/* Authors:
 *      Sebastien Lambla (seb@serialseb.com)
 * Copyright:
 *      (C) 2007-2009 Caffeine IT & naughtyProd Ltd (http://www.caffeine-it.com)
 * License:
 *      This file is distributed under the terms of the MIT License found at the end of this file.
 */

#endregion

using System;
using System.Collections.Specialized;
using System.Linq;
using OpenRasta.Collections.Specialized;
using OpenRasta.DI;
using OpenRasta.TypeSystem.ReflectionBased;

namespace OpenRasta.Web
{
  // ReSharper disable once InconsistentNaming - Legacy code
  public static class IUriResolverExtensions
  {
    [Obsolete]
    public static Uri CreateUri(this object target)
    {
      return CreateUri(target, (string) null);
    }

    [Obsolete]
    public static Uri CreateUri(this object target, string uriName)
    {
      return target.CreateUri(uriName, null);
    }

    [Obsolete]
    public static Uri CreateUri(this object target, string uriName, object additionalProperties)
    {
      return target.CreateUri(DependencyManager.GetService<ICommunicationContext>().ApplicationBaseUri, uriName,
        additionalProperties);
    }

    [Obsolete]
    public static Uri CreateUri(this object target, object additionalProperties)
    {
      return target.CreateUri((string) null, additionalProperties);
    }

    [Obsolete]
    public static Uri CreateUri(this object target, Uri baseUri)
    {
      return target.CreateUri(baseUri, null);
    }

    [Obsolete]
    public static Uri CreateUri(this object target, Uri baseUri, string uriName)
    {
      return target.CreateUri(baseUri, uriName, null);
    }

    [Obsolete]
    public static Uri CreateUri(this object target, Uri baseUri, object additionalProperties)
    {
      return target.CreateUri(baseUri, null, additionalProperties);
    }

    [Obsolete]
    public static Uri CreateUri(this object target, Uri baseUri, string uriName, object additionalProperties)
    {
      if (target == null)
        throw new ArgumentNullException("target");
      var uriResolver = DependencyManager.GetService<IUriResolver>();
      return CreateUriCore(target, baseUri, uriName, additionalProperties, uriResolver);
    }

    static Uri CreateUriCore(object target, Uri baseUri, string uriName, object additionalProperties,
      IUriResolver uriResolver)
    {
      if (target is Type targetType)
        return uriResolver.CreateUriFor(baseUri, targetType, uriName, additionalProperties?.ToNameValueCollection());

      var props = target.ToNameValueCollection();
      return uriResolver.CreateUriFor(baseUri, target.GetType(), uriName, Merge(props, additionalProperties));
    }

    public static Uri CreateUriFor<T>(this IUriResolver resolver)
    {
      return resolver.CreateUriFor(typeof(T));
    }

    public static Uri CreateFrom<T>(this IUriResolver resolver, T resourceInstance, Uri baseUri)
    {
      return CreateUriCore(resourceInstance, baseUri, null, null, resolver);
    }

    public static Uri CreateUriFor(this IUriResolver resolver, Type type)
    {
      return resolver.CreateUriFor(type, null);
    }

    public static Uri CreateUriFor(this IUriResolver resolver, Type type, object keyValues)
    {
      return resolver.CreateUriFor(type, keyValues?.ToNameValueCollection());
    }

    public static Uri CreateUriFor(this IUriResolver resolver, Type type, NameValueCollection keyValues)
    {
      return resolver.CreateUriFor(type, null, keyValues);
    }

    public static Uri CreateUriFor(this IUriResolver resolver, Type type, string uriName, object keyValues)
    {
      return resolver.CreateUriFor(type, uriName, keyValues?.ToNameValueCollection());
    }

    [Obsolete]
    public static Uri CreateUriFor(this IUriResolver resolver, Type type, string uriName, NameValueCollection keyValues)
    {
      return resolver.CreateUriFor(DependencyManager.GetService<ICommunicationContext>().ApplicationBaseUri, type,
        uriName,
        keyValues);
    }

    public static Uri CreateUriFor(this IUriResolver resolver, Uri baseAddress, Type type)
    {
      return resolver.CreateUriFor(baseAddress, type, (string) null);
    }

    public static Uri CreateUriFor(this IUriResolver resolver, Uri baseAddress, Type type, string uriName)
    {
      return resolver.CreateUriFor(baseAddress, type, uriName, (NameValueCollection) null);
    }

    public static Uri CreateUriFor(this IUriResolver resolver, Uri baseAddress, Type type, object nameValues)
    {
      return resolver.CreateUriFor(baseAddress, type, nameValues != null ? nameValues.ToNameValueCollection() : null);
    }

    public static Uri CreateUriFor(this IUriResolver resolver, Uri baseAddress, Type resourceType,
      NameValueCollection nameValues)
    {
      return resolver.CreateUriFor(baseAddress, resourceType, "", nameValues);
    }


    static NameValueCollection Merge(NameValueCollection source, object target)
    {
      if (target == null)
        return source;
      if (source == null)
        source = new NameValueCollection();
      if (target is NameValueCollection)
        source.AddReplace((NameValueCollection) target);
      else
        source.AddReplace(target.ToNameValueCollection());
      return source;
    }
  }
}

#region Full license

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#endregion