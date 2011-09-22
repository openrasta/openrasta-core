using System;

namespace OpenRasta.Configuration.Fluent
{
    public interface ICodecParentDefinition :INoIzObject
    {
        ICodecDefinition TranscodedBy<TCodec>(object configuration)
            where TCodec : Codecs.ICodec;

        ICodecDefinition TranscodedBy(Type type, object configuration);
    }
    public interface ICodecParentDefinition<TResource> : ICodecParentDefinition
    {
        new ICodecDefinition<TResource,TCodec> TranscodedBy<TCodec>(object configuration)
            where TCodec : Codecs.ICodec;
    }
}