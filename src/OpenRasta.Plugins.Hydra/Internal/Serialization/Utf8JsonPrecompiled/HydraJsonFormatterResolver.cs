using Utf8Json;
using Utf8Json.Resolvers;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public class HydraJsonFormatterResolver : IJsonFormatterResolver
  {
    static readonly IJsonFormatterResolver _resolver =
      CompositeResolver.Create(new IJsonFormatter[] {new ContextFormatter()},
                                                            new[] {StandardResolver.ExcludeNullCamelCase});

    public IJsonFormatter<T> GetFormatter<T>()
    {
      return _resolver.GetFormatter<T>();
    }
  }
}