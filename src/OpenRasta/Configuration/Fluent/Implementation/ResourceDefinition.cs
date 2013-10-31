using System;
using System.Globalization;
using OpenRasta.Codecs;
using OpenRasta.Configuration.Fluent.Extensions;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.TypeSystem;

namespace OpenRasta.Configuration.Fluent.Implementation
{
    public class ResourceDefinition<T> : ResourceDefinition, IResourceDefinition<T>
    {
        public ResourceDefinition(IFluentTarget rootTarget, ITypeSystem typeSystem, ResourceModel resourceRegistration) : base(rootTarget, typeSystem, resourceRegistration)
        {
        }

        public new IUriDefinition<T> AtUri(string uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            var uriModel = new UriModel { Uri = uri };
            Resource.Uris.Add(uriModel);

            return new UriDefinition<T>(this, uriModel);
        }

        class UriDefinition<TResource> : UriDefinition, IUriDefinition<TResource>
        {
            readonly ResourceDefinition<TResource> _resourceDefinition;

            public UriDefinition(ResourceDefinition<TResource> resourceDefinition, UriModel uriModel) : base(resourceDefinition, uriModel)
            {
                _resourceDefinition = resourceDefinition;
            }

            public new IResourceDefinition<TResource> And
            {
                get { return _resourceDefinition; }
            }
        }
    }
    public class ResourceDefinition : IResourceDefinition,
                                      IResourceTarget,
                                      IHandlerParentDefinition, 
                                      IHandlerForResourceWithUriDefinition,
                                      IHandlerTarget,
                                      IHandler
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
            UriModel model = new UriModel { Uri = uri };
            Resource.Uris.Add(model);

            return new UriDefinition(this, model);
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

        protected abstract class TargetWrapper : IResourceTarget
        {
            readonly ResourceDefinition _target;

            public TargetWrapper(ResourceDefinition target)
            {
                _target = target;
            }

            public ResourceModel Resource
            {
                get { return _target.Resource; }
            }

            public ITypeSystem TypeSystem
            {
                get { return _target.TypeSystem; }
            }

            public IMetaModelRepository Repository
            {
                get { return _target.Repository; }
            }
        }

        protected class UriDefinition : TargetWrapper, IUriDefinition, IUriTarget
        {
            readonly ResourceDefinition _resourceDefinition;
            readonly UriModel _uriModel;

            public UriDefinition(ResourceDefinition resourceDefinition, UriModel uriModel) : base(resourceDefinition)
            {
                _resourceDefinition = resourceDefinition;
                _uriModel = uriModel;
            }

            public IResourceDefinition And
            {
                get { return _resourceDefinition; }
            }

            public IHandlerForResourceWithUriDefinition HandledBy<T>()
            {
                return _resourceDefinition.HandledBy<T>();
            }

            public IHandlerForResourceWithUriDefinition HandledBy(Type type)
            {
                return _resourceDefinition.HandledBy(type);
            }

            public IHandlerForResourceWithUriDefinition HandledBy(IType type)
            {
                return _resourceDefinition.HandledBy(type);
            }

            public IUriDefinition InLanguage(string language)
            {
                _uriModel.Language = language == null
                                         ? CultureInfo.InvariantCulture
                                         : CultureInfo.GetCultureInfo(language);
                return this;
            }

            public IUriDefinition Named(string uriName)
            {
                _uriModel.Name = uriName;
                return this;
            }

            public UriModel Uri
            {
                get { return _uriModel; }
            }
        }

    }
}