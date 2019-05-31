using Utf8Json;
using Utf8Json.Resolvers;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public class CustomResolver : IJsonFormatterResolver
  {
    readonly IJsonFormatterResolver _resolver;

    public CustomResolver()
    {
      _resolver = CompositeResolver.Create(new IJsonFormatter[] {new ContextFormatter()},
        new[] {StandardResolver.CamelCase});
    }

    public IJsonFormatter<T> GetFormatter<T>()
    {
      return _resolver.GetFormatter<T>();
    }
  }
}