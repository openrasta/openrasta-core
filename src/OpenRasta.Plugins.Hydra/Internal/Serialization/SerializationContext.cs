using System;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization
{
  public class SerializationContext
  {
    public Uri BaseUri { get; set; }
    
    public Func<object,string> UriGenerator { get; set; }
  }
}