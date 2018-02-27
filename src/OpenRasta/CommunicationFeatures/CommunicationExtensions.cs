using OpenRasta.Web;

namespace OpenRasta.CommunicationFeatures
{
  public static class CommunicationExtensions
  {
    public static void Feature<T>(this ICommunicationContext context, T feature)
    {
      context.PipelineData[GetFeatureKey<T>()] = feature;
    }
    public static bool Feature<T>(this ICommunicationContext context, out T feature) where T : class
    {
      if (!context.PipelineData.TryGetValue(GetFeatureKey<T>(), out var untypedFeature))
      {
        feature = null;
        return false;
      }

      feature = (T) untypedFeature;
      return true;
    }

    static string GetFeatureKey<T>()
    {
      return $"openrasta.{typeof(T).Name}";
    }
  }
}