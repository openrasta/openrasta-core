using System.Collections.Generic;
using OpenRasta.Codecs.Newtonsoft.Json;
using OpenRasta.Configuration;

namespace OpenRastaDemo
{
    public class DemoConfigurationSource : IConfigurationSource
    {
        public void Configure()
        {
            ResourceSpace.Has
                .ResourcesOfType<IEnumerable<RootResponse>>()
                .AtUri("/")
                .Named("root")
                .HandledBy<RootHandler>()
                .TranscodedBy<NewtonsoftJsonCodec>()
                .ForMediaType("application/json")
                ;

            ResourceSpace.Has
                .ResourcesOfType<RootResponse>()
                .AtUri("/littlejson")
                .Named("littlejson")
                .HandledBy<LittleJsonHandler>()
                .TranscodedBy<NewtonsoftJsonCodec>()
                .ForMediaType("application/json")
                ;
        }
    }
}