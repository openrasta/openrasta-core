using System;
using OpenRasta.Configuration.Fluent.Extensions;
using OpenRasta.Configuration.Fluent.Implementation;
using OpenRasta.TypeSystem;

namespace OpenRasta.Configuration.Fluent
{
  public interface IHandlerParentDefinition : INoIzObject
  {
    IHandlerForResourceWithUriDefinition HandledBy<T>();
    IHandlerForResourceWithUriDefinition HandledBy(Type type);
    IHandlerForResourceWithUriDefinition HandledBy(IType type);
  }
  
  /// <summary>
  /// These exist for the sole purpose of providing support for the new fluent extensibility APIs
  /// over the old API and not loose chaining. It's a mess, *I know*, but it's the best we can
  /// hope for for this version. 
  /// </summary>
  public static class FluentExtensionExtensions
  {
    public static IUriDefinition AtUri(this IResource resource, string uri)
    {
      if (uri == null) throw new ArgumentNullException("uri");

      var hackedBaseType = resource as IResourceDefinition;

      if (hackedBaseType == null)
        throw new InvalidOperationException("Something has gone wonky in the compat API. <sigh>");

      return hackedBaseType.AtUri(uri);
    }
  }
}