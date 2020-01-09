using System;
using OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization
{
  public class SerializationContext
  {
    public Uri BaseUri { get; set; }
    
    public Func<object,string> UriGenerator { get; set; }
    
    public Func<object,string> TypeGenerator { get; set; }
    
  }
}