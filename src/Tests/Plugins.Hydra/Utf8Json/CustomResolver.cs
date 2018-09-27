using Utf8Json;
using Utf8Json.Resolvers;

namespace Tests.Plugins.Hydra.Utf8Json
{
  public class CustomResolver : IJsonFormatterResolver
  {
    public CustomResolver()
    {
    }

    public static IJsonFormatterResolver Instance { get; } = new CustomResolver();

    public IJsonFormatter<T> GetFormatter<T>()
    {
      return BuiltinResolver.Instance.GetFormatter<T>();
    }
  }
}