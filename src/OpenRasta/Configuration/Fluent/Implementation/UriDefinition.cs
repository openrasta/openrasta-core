using System;
using System.Globalization;
using OpenRasta.Configuration.Fluent.Extensions;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.TypeSystem;

namespace OpenRasta.Configuration.Fluent.Implementation
{
    public class UriDefinition<TResource> : UriDefinition, IUriDefinition<TResource>
    {
        readonly ResourceDefinition<TResource> _resourceDefinition;

        public UriDefinition(IFluentTarget rootTarget, ResourceDefinition<TResource> resourceDefinition, string uri) : base(rootTarget, resourceDefinition, uri)
        {
            _resourceDefinition = resourceDefinition;
        }


        IResourceDefinition<TResource> IUriDefinition<TResource>.And
        {
            get { return _resourceDefinition; }
        }

        public new IHandlerForResourceWithUriDefinition<TResource, THandler> HandledBy<THandler>()
        {
            return _resourceDefinition.HandledBy<THandler>();
        }
    }
    public class UriDefinition : IUriDefinition, IUriTarget
    {
        readonly IFluentTarget _rootTarget;
        readonly ResourceDefinition _resourceDefinition;
        readonly UriModel _uriModel;

        public UriDefinition(IFluentTarget rootTarget, ResourceDefinition resourceDefinition, string uri)
        {
            _rootTarget = rootTarget;
            _resourceDefinition = resourceDefinition;
            _uriModel = new UriModel { Uri = uri };
            _resourceDefinition.Resource.Uris.Add(_uriModel);
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
            get { return _resourceDefinition.Resource; }
        }

        public UriModel Uri
        {
            get { return _uriModel; }
        }
    }
}