using OpenRasta.TypeSystem.ReflectionBased;
using OpenRasta.TypeSystem.Surrogated;
using OpenRasta.TypeSystem.Surrogates;
using OpenRasta.TypeSystem.Surrogates.Static;

namespace OpenRasta.TypeSystem
{
    public static class TypeSystems
    {
      public static ITypeSystem Default { get; } = new ReflectionBasedTypeSystem(
          new SurrogateBuilderProvider(
            new ISurrogateBuilder[]
            {
              new DateTimeSurrogate(),
              new ListIndexerSurrogateBuilder(),
              new CollectionIndexerSurrogateBuilder()
            }),
          new PathManager());
    }
}