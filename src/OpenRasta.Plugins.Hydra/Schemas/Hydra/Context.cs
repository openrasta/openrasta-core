using System;
using System.Collections.Generic;

namespace OpenRasta.Plugins.Hydra.Schemas
{
  public static partial class HydraCore
  {
    public class Context
    {
      public Dictionary<string, Uri> Curies { get; set; }
      public Dictionary<string, string> Classes { get; set; }
      public string DefaultVocabulary { get; set; }
    }
  }
}