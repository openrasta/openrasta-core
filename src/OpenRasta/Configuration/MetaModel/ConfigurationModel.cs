using System;
using System.Collections.Generic;

namespace OpenRasta.Configuration.MetaModel
{
  public class ConfigurationModel
  {
    protected ConfigurationModel()
    {
      Properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    }

    public IDictionary<string, object> Properties { get; }
  }
}