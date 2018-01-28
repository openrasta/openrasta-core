using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.Configuration.MetaModel
{
  public class ResourceModel : ConfigurationModel
  {
    public ResourceModel()
    {
      Uris = new List<UriModel>();
      Handlers = new List<HandlerModel>();
      Codecs = new List<CodecModel>();
    }

    public IList<CodecModel> Codecs { get; }
    public IList<HandlerModel> Handlers { get; }

    public bool IsStrictRegistration { get; set; }
    public object ResourceKey { get; set; }
    public IList<UriModel> Uris { get; }


    public override string ToString()
    {
      return
        $"ResourceKey: {ResourceKey}, Uris: {Uris.Aggregate(string.Empty, (str, reg) => str + reg + ";")}, Handlers: {Handlers.Aggregate(string.Empty, (str, reg) => str + reg + ";")}, Codecs: {Codecs.Aggregate(string.Empty, (str, reg) => str + reg + ";")}";
    }
  }
}