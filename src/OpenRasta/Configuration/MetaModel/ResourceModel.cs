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

        public IList<CodecModel> Codecs { get; private set; }
        public IList<HandlerModel> Handlers { get; private set; }

        public bool IsStrictRegistration { get; set; }
        public object ResourceKey { get; set; }
        public IList<UriModel> Uris { get; private set; }


        public override string ToString()
        {
            return string.Format("ResourceKey: {0}, Uris: {1}, Handlers: {2}, Codecs: {3}", 
                                 ResourceKey, 
                                 Uris.Aggregate(string.Empty, (str, reg) => str + reg + ";"), 
                                 Handlers.Aggregate(string.Empty, (str, reg) => str + reg + ";"), 
                                 Codecs.Aggregate(string.Empty, (str, reg) => str + reg + ";"));
        }
    }
}