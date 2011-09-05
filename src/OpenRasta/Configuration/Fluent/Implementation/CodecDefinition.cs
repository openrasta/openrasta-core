JAAAAAAAAAAAAAAAMONERO
JAMONERO
JAMO
using System;
using OpenRasta.Configuration.Fluent.Extensions;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Configuration.Fluent.Implementation
{
    public class CodecDefinition : ICodecDefinition, ICodecTarget
    {
        readonly IFluentTarget _rootTarget;
        readonly CodecModel _codecRegistration;

        public CodecDefinition(IFluentTarget rootTarget, ResourceDefinition resourceDefinition, Type codecType, object configuration)
        {
            _rootTarget = rootTarget;
            ResourceDefinition = resourceDefinition;
            _codecRegistration = new CodecModel(codecType, configuration);
            ResourceDefinition.Resource.Codecs.Add(_codecRegistration);
        }

        public ICodecParentDefinition And
        {
            get { return ResourceDefinition; }
        }

        public ResourceDefinition ResourceDefinition { get; set; }

        public ICodecWithMediaTypeDefinition ForMediaType(MediaType mediaType)
        {
            var model = new MediaTypeModel { MediaType = mediaType };
            _codecRegistration.MediaTypes.Add(model);

            return new CodecMediaTypeDefinition(this, model);
        }

        public IMetaModelRepository Repository
        {
            get { return _rootTarget.Repository; }
        }

        public ITypeSystem TypeSystem
        {
            get { return _rootTarget.TypeSystem; }
        }

        public ResourceModel Resource
        {
            get { return ResourceDefinition.Resource; }
        }

        public CodecModel Codec
        {
            get { return _codecRegistration; }
        }
    }
}