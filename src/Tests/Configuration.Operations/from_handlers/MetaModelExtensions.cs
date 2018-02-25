using System.Collections.Generic;
using System.Linq;
using OpenRasta.Configuration.MetaModel;

namespace Tests.Scenarios.HandlerSelection.Configuration.Operations
{
  public static class MetaModelExtensions
  {
    public static IEnumerable<ResourceModel> ByName(this IEnumerable<ResourceModel> resources, string name)
    {
      return resources.Where(r => (string) r.ResourceKey == name);
    }
  }
}