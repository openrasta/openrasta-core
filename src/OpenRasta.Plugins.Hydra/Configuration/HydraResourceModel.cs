using System.Collections.Generic;
using OpenRasta.Plugins.Hydra.Schemas.Hydra;

namespace OpenRasta.Plugins.Hydra.Configuration
{
  public class HydraResourceModel
  {
    public Vocabulary Vocabulary { get; set; }
    public List<Operation> SupportedOperations { get; set; } = new List<Operation>();
  }
}