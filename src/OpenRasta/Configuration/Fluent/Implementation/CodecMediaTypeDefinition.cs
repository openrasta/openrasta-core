using System;
using OpenRasta.Configuration.Fluent.Extensions;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Configuration.Fluent.Implementation
{
    public class CodecMediaTypeDefinition : ICodecWithMediaTypeDefinition, IMediaTypeTarget
    {
        readonly MediaTypeModel _model;
        readonly CodecDefinition _parent;

        public CodecMediaTypeDefinition(CodecDefinition parent, MediaTypeModel model)
        {
            _parent = parent;
            _model = model;
        }

        public ICodecParentDefinition And
        {
            get { return _parent.And; }
        }

        public ICodecWithMediaTypeDefinition ForMediaType(MediaType mediaType)
        {
            return _parent.ForMediaType(mediaType);
        }

        public ICodecWithMediaTypeDefinition ForExtension(string extension)
        {
            _model.Extensions.Add(extension);
            return this;
        }

        public IMetaModelRepository Repository
        {
            get { return _parent.Repository; }
        }

        public ITypeSystem TypeSystem
        {
            get { return _parent.TypeSystem; }
        }

        public ResourceModel Resource
        {
            get { return _parent.Resource; }
        }

        public MediaTypeModel MediaType
        {
            get { return _model; }
        }
    }
}