using System;
using OpenRasta.Codecs;
using OpenRasta.Configuration.Fluent.Extensions;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Configuration.Fluent.Implementation
{
    public class ResourceDefinition<TResource> : ResourceDefinition, 
        IResourceDefinition<TResource>,
        IHandlerParentDefinition<TResource>,
        ICodecParentDefinition<TResource>
    {
        public ResourceDefinition(IFluentTarget rootTarget, ITypeSystem typeSystem, ResourceModel resourceRegistration) : base(rootTarget, typeSystem, resourceRegistration)
        {
        }

        public ICodecParentDefinition<TResource> WithoutUri
        {
            get { return new CodecParentDefinition<TResource>(this); }
        }

        public IUriDefinition<TResource> AtUri(string uri)
        {
            return new UriDefinition<TResource>(_rootTarget, this, uri);
        }

        public ICodecDefinition<TResource, TCodec> TranscodedBy<TCodec>(object configuration) where TCodec : Codecs.ICodec
        {
            return new CodecDefinition<TCodec>(_rootTarget, this, typeof(TCodec), configuration);
        }

        public IHandlerForResourceWithUriDefinition<TResource, THandler> HandledBy<THandler>()
        {
            return new HandlerDefinition<THandler>(this);
        }
        public class HandlerDefinition< THandler> :
            IHandlerForResourceWithUriDefinition<TResource, THandler>,
            IHandler<TResource, THandler>
        {
            readonly ResourceDefinition<TResource> _resourceDefinition;

            public HandlerDefinition(ResourceDefinition<TResource> resourceDefinition)
            {
                _resourceDefinition = resourceDefinition;
            }


            public IHandlerParentDefinition<TResource> And
            {
                get { return _resourceDefinition; }
            }

            IHandlerParentDefinition IHandlerForResourceWithUriDefinition.And
            {
                get { return _resourceDefinition; }
            }


            public ICodecDefinition<TResource, TCodec> TranscodedBy<TCodec>(object configuration) where TCodec : Codecs.ICodec
            {
                return _resourceDefinition.TranscodedBy<TCodec>(configuration);
            }

            ICodecDefinition ICodecParentDefinition.TranscodedBy<TCodec>(object configuration)
            {
                return  TranscodedBy<TCodec>(configuration);
            }

            public ICodecDefinition TranscodedBy(Type type, object configuration)
            {
                return _resourceDefinition.TranscodedBy(type, configuration);
            }
        }

        public class CodecDefinition<TCodec>
            : CodecDefinition,
              ICodecDefinition<TResource, TCodec>,
              ICodec<TResource, TCodec>
        {
            readonly ResourceDefinition<TResource> _resourceDefinition;

            public CodecDefinition(IFluentTarget rootTarget, ResourceDefinition<TResource> resourceDefinition, Type codecType, object configuration)
                : base(rootTarget, resourceDefinition, codecType, configuration)
            {
                _resourceDefinition = resourceDefinition;
            }

            public ICodecWithMediaTypeDefinition<TResource, TCodec> ForMediaType(MediaType mediaType)
            {
            var model = new MediaTypeModel { MediaType = mediaType };
            Codec.MediaTypes.Add(model);

                return new CodecMediaTypeDefinition<TResource, TCodec>(this, model);
            }

            public new ICodecParentDefinition<TResource> And
            {
                get { return _resourceDefinition; }
            }
        }

        public class CodecMediaTypeDefinition<TResource, TCodec>
            : CodecMediaTypeDefinition,
              ICodecWithMediaTypeDefinition<TResource, TCodec>,
              IMediaType<TResource, TCodec>
        {
            public CodecMediaTypeDefinition(CodecDefinition<TCodec> parent, MediaTypeModel model)
                : base(parent, model)
            {
            }

            public ICodecWithMediaTypeDefinition<TResource, TCodec> ForExtension(string extension)
            {
                base.ForExtension(extension);
                return this;
            }
        }
    }

    public class ResourceDefinition : IResourceDefinition,
                                      IHandlerParentDefinition, 
                                      IHandlerForResourceWithUriDefinition,
                                      IHandlerTarget
    {
        protected readonly IFluentTarget _rootTarget;
        protected readonly ITypeSystem _typeSystem;
        HandlerModel _lastHandlerModel;

        public ResourceDefinition(IFluentTarget rootTarget, ITypeSystem typeSystem, ResourceModel resourceRegistration)
        {
            Resource = resourceRegistration;
            _rootTarget = rootTarget;
            _typeSystem = typeSystem;
        }

        public IHandlerParentDefinition And
        {
            get { return this; }
        }


        /// <exception cref="InvalidOperationException">Cannot make a resource URI-less if a URI is already registered.</exception>
        public ICodecParentDefinition WithoutUri
        {
            get
            {
                // TODO: remove the restriction
                if (Resource.Uris.Count > 0)
                    throw new InvalidOperationException(
                        "Cannot make a resource URI-less if a URI is already registered.");
                return new CodecParentDefinition(this);
            }
        }

        public ICodecDefinition TranscodedBy<TCodec>(object configuration) where TCodec : Codecs.ICodec
        {
            return new CodecDefinition(_rootTarget, this, typeof(TCodec), configuration);
        }

        public ICodecDefinition TranscodedBy(Type codecType, object configuration)
        {
            return new CodecDefinition(_rootTarget, this, codecType, configuration);
        }

        public IHandlerForResourceWithUriDefinition HandledBy<T>()
        {
            return HandledBy(_typeSystem.FromClr(typeof(T)));
        }

        public IHandlerForResourceWithUriDefinition HandledBy(Type type)
        {
            return HandledBy(_typeSystem.FromClr(type));
        }

        public IHandlerForResourceWithUriDefinition HandledBy(IType type)
        {
            if (type == null) throw new ArgumentNullException("type");
            Resource.Handlers.Add(_lastHandlerModel = new HandlerModel(type));
            return this;
        }

        /// <exception cref="ArgumentNullException"><c>uri</c> is null.</exception>
        public IUriDefinition AtUri(string uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            return new UriDefinition(_rootTarget, this, uri);
        }

        public IMetaModelRepository Repository
        {
            get { return _rootTarget.Repository; }
        }

        public ITypeSystem TypeSystem
        {
            get { return _rootTarget.TypeSystem; }
        }

        public ResourceModel Resource { get; private set; }

        public HandlerModel Handler
        {
            get { return _lastHandlerModel; }
        }
    }
}