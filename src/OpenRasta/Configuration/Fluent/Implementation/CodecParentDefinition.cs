using System;
using OpenRasta.Codecs;

namespace OpenRasta.Configuration.Fluent.Implementation
{
    public class CodecParentDefinition : ICodecParentDefinition
    {
        readonly ResourceDefinition _resourceDefinition;

        public CodecParentDefinition(ResourceDefinition registration)
        {
            _resourceDefinition = registration;
        }

        public ICodecDefinition TranscodedBy<TCodec>(object configuration) where TCodec : Codecs.ICodec
        {
            return _resourceDefinition.TranscodedBy<TCodec>(configuration);
        }

        public ICodecDefinition TranscodedBy(Type type, object configuration)
        {
            return _resourceDefinition.TranscodedBy(type, configuration);
        }
    }
    public class CodecParentDefinition<TResource> : CodecParentDefinition, ICodecParentDefinition<TResource>
    {
        readonly ResourceDefinition<TResource> _registration;

        public CodecParentDefinition(ResourceDefinition<TResource> registration) : base(registration)
        {
            _registration = registration;
        }

        public ICodecDefinition<TResource, TCodec> TranscodedBy<TCodec>(object configuration) where TCodec : Codecs.ICodec
        {
            return _registration.TranscodedBy<TCodec>(configuration);
        }
    }
}