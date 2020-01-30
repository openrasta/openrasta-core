using System;
using System.Globalization;
using System.Linq.Expressions;
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
      if (uri == null) throw new ArgumentNullException(nameof(uri));
      var uriModel = new UriModel { Uri = uri, ResourceModel = Resource };
      Resource.Uris.Add(uriModel);

      return new UriDefinition<T>(this, uriModel);
    }

    public IUriDefinition<T> AtUri(Expression<Func<T, string>> uri)
    {
      if (uri == null) throw new ArgumentNullException(nameof(uri));
      var compiled = uri.Compile();
      string compiledUntyped(object resource) => compiled((T) resource);
      var uriModel = new UriModel
      {
        Uri = new UriExpressionVisitor().GenerateUri(typeof(T), uri),
        ResourceModel = Resource,
        Properties = { ["compiled"] = (Func<object, string>) compiledUntyped }
      };
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

      public new IResourceDefinition<TResource> And => _resourceDefinition;
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

    public IHandlerParentDefinition And => this;


    /// <exception cref="InvalidOperationException">Cannot make a resource URI-less if a URI is already registered.</exception>
    public ICodecParentDefinition WithoutUri => new CodecParentDefinition(this);

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
      if (type == null) throw new ArgumentNullException(nameof(type));
      Resource.Handlers.Add(_lastHandlerModel = new HandlerModel(type));
      return this;
    }

    /// <exception cref="ArgumentNullException"><c>uri</c> is null.</exception>
    public IUriDefinition AtUri(string uri)
    {
      if (uri == null) throw new ArgumentNullException(nameof(uri));
      UriModel model = new UriModel { Uri = uri,ResourceModel = this.Resource};
      Resource.Uris.Add(model);

      return new UriDefinition(this, model);
    }

    public IMetaModelRepository Repository => _rootTarget.Repository;

    public ITypeSystem TypeSystem => _rootTarget.TypeSystem;

    public ResourceModel Resource { get; private set; }

    public HandlerModel Handler => _lastHandlerModel;

    public class UriDefinition : TargetWrapper, IUriDefinition, IUriTarget
    {
      readonly ResourceDefinition _resourceDefinition;
      readonly UriModel _uriModel;

      public UriDefinition(ResourceDefinition resourceDefinition, UriModel uriModel) : base(resourceDefinition)
      {
        _resourceDefinition = resourceDefinition;
        _uriModel = uriModel;
      }

      public IResourceDefinition And => _resourceDefinition;

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

      public UriModel Uri => _uriModel;
    }
  }
}