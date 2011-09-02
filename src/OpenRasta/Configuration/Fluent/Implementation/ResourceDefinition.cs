JAAAAAAAAAAAAAAAMONERO
JAMONERO
JAMO
using System;
using OpenRasta.Codecs;
using OpenRasta.Configuration.Fluent.Extensions;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.TypeSystem;

namespace OpenRasta.Configuration.Fluent.Implementation
{
    public class ResourceDefinition : IResourceDefinition,
                                      IResourceTarget,
                                      IHandlerParentDefinition, 
                                      IHandlerForResourceWithUriDefinition,
                                      IHandlerTarget
    {
        readonly IFluentTarget _rootTarget;
        readonly ITypeSystem _typeSystem;
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