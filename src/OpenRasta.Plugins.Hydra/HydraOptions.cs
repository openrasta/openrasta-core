using System;
using System.Collections.Generic;
using OpenRasta.Configuration;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled;

namespace OpenRasta.Plugins.Hydra
{
  public class HydraOptions
  {
    public HydraOptions()
    {
      Serializer = ctx => ctx.Transient(() => new PreCompiledUtf8JsonSerializer()).As<IMetaModelHandler>();
    }

    public IList<Vocabulary> Curies { get; } = new List<Vocabulary>();
    public Vocabulary Vocabulary { get; set; }
    public Action<ITypeRegistrationContext> Serializer { get; set; }
  }
}