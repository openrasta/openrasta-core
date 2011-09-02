using System;
using System.Collections.Generic;

namespace OpenRasta.Configuration.MetaModel
{
    public class ConfigurationModel
    {
        public ConfigurationModel()
        {
            Properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public IDictionary<string, object> Properties { get; private set; }
    }
}