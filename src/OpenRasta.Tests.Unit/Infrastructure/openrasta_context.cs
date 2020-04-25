using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenRasta.Codecs;
using OpenRasta.Collections;
using OpenRasta.Concordia;
using OpenRasta.Diagnostics;
using OpenRasta.DI;
using OpenRasta.Handlers;
using OpenRasta.Hosting;
using OpenRasta.Hosting.Compatibility;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Pipeline;
using OpenRasta.Security;
using OpenRasta.TypeSystem;
using OpenRasta.Web;
using OpenRasta.Web.Internal;
using Shouldly;

namespace OpenRasta.Tests.Unit.Infrastructure
{
  public abstract class openrasta_context : context
  {
    Dictionary<Type, Func<ICommunicationContext, Task<PipelineContinuation>>> _actions;
    InMemoryHost Host;
    IDisposable _requestScope;
    ContextScope _ambientContext;

    protected openrasta_context()
    {
      TypeSystem = TypeSystems.Default;
    }

    protected PipelineContinuation Result { get; set; }

    protected ICodecRepository Codecs => Resolver.Resolve<ICodecRepository>();

    protected WriteTrackingResponseCommunicationContext Context { get; private set; }
    protected bool IsContributorExecuted { get; set; }
    protected IPipeline Pipeline { get; private set; }

    protected InMemoryRequest Request => Context.Request as InMemoryRequest;

    protected IDependencyResolver Resolver => Host.Resolver;

    protected ITypeSystem TypeSystem { get; set; }

    protected IUriResolver UriResolver => Resolver.Resolve<IUriResolver>();

    public void given_dependency<TInterface>(TInterface instance)
    {
      Resolver.AddDependencyInstance(typeof(TInterface), instance, DependencyLifetime.Singleton);
    }

    public T given_pipeline_contributor<T>() where T : class, IPipelineContributor
    {
      return given_pipeline_contributor<T>(null);
    }

    public T given_pipeline_contributor<T>(Func<T> constructor) where T : class, IPipelineContributor
    {
      Pipeline = new SinglePipeline<T>(constructor, Resolver, _actions);
      Pipeline.Contributors[0].Initialize(Pipeline);
      return (T)Pipeline.Contributors[0];
    }

    public void given_uri_registration<T>(string uri, string uriName = null)
    {
      UriResolver.Add(new UriRegistration(uri, TypeSystem.FromClr(typeof(T)), uriName, CultureInfo.CurrentCulture));
    }

    public void given_uri_registration(object key, string uri, string uriName = null)
    {
      UriResolver.Add(new UriRegistration(uri, key, uriName, CultureInfo.CurrentCulture));
    }

    public void given_request_uri(string uri)
    {
      Context.Request.Uri = new Uri(uri);
    }

    public void then_contributor_is_executed()
    {
      IsContributorExecuted.ShouldBeTrue();
    }

    public PipelineContinuation when_sending_notification<TTrigger>()
    {
      var actionFound = _actions.ContainsKey(typeof(TTrigger));
      if (actionFound == false && _actions.Count > 0)
      {
        throw new InvalidOperationException("No action for trigger");
      }

      var action = _actions.Count == 0 ? Pipeline.CallGraph.FirstOrDefault()?.Action : _actions[typeof(TTrigger)];
      if (action == null)
        throw new InvalidOperationException($"Could not find operation of type '{nameof(TTrigger)}'.");

      Result = action(Context).GetAwaiter().GetResult();
      IsContributorExecuted = true;
      return Result;
    }

    protected void given_pipeline_resourceKey<T1>()
    {
      Context.PipelineData.ResourceKey = typeof(T1).AssemblyQualifiedName;
    }


    protected void given_pipeline_selectedHandler<THandler>()
    {
      if (Context.PipelineData.SelectedHandlers == null)
        Context.PipelineData.SelectedHandlers = new List<IType>();

      Context.PipelineData.SelectedHandlers.Add(TypeSystem.FromClr<THandler>());
    }

    protected void given_pipeline_uriparams(NameValueCollection nameValueCollection)
    {
      if (Context.PipelineData.SelectedResource == null)
        Context.PipelineData.SelectedResource = new UriRegistration(null, null);
      Context.PipelineData.SelectedResource.UriTemplateParameters.Add(nameValueCollection);
    }

    protected void given_registration_codec<TCodec>()
    {
      CodecRegistration.FromCodecType(typeof(TCodec), TypeSystem).ForEach(x => Codecs.Add(x));
    }

    protected void given_registration_codec<TCodec, TResource>(string mediaTypes)
    {
      foreach (var contentType in MediaType.Parse(mediaTypes))
        Codecs.Add(CodecRegistration.FromResourceType(typeof(TResource),
            typeof(TCodec),
            TypeSystem,
            contentType,
            null,
            null,
            false));
    }

    protected void given_registration_handler<TResource, THandler>()
    {
      Resolver.Resolve<IHandlerRepository>().AddResourceHandler(typeof(TResource).AssemblyQualifiedName,
          TypeSystem.FromClr
              (typeof(THandler)));
    }

    protected void given_request_entity_body(byte[] bytes)
    {
      Request.Entity = new HttpEntity(new HttpHeaderDictionary(), new MemoryStream(bytes))
      {
          ContentLength = bytes.Length
      };
    }

    protected void given_request_entity_body(string content)
    {
      var bytes = Encoding.UTF8.GetBytes(content);
      Request.Entity = new HttpEntity(Request.Entity.Headers, new MemoryStream(bytes)) { ContentLength = bytes.Length };
    }

    protected void given_request_header_accept(string p)
    {
      Context.Request.Headers["Accept"] = p;
    }

    protected void given_request_header_content_type(string mediaType)
    {
      if (mediaType == null)
        Context.Request.Entity.ContentType = null;
      else
        Context.Request.Entity.ContentType = new MediaType(mediaType);
    }

    protected void given_request_header_content_type(MediaType mediaType)
    {
      Context.Request.Entity.ContentType = mediaType;
    }

    protected void given_request_httpmethod(string method)
    {
      Context.Request.HttpMethod = method;
    }

    protected void given_response_entity(object responseEntity, Type codecType, string contentType)
    {
      Context.Response.Entity.ContentType = new MediaType(contentType);

      Context.PipelineData.ResponseCodec =
          CodecRegistration.FromResourceType(responseEntity?.GetType() ?? typeof(object),
              codecType,
              TypeSystem,
              new MediaType(contentType),
              null,
              null,
              false);
      given_response_entity(responseEntity);
    }

    protected void given_response_entity(object responseEntity)
    {
      Context.Response.Entity.Instance = responseEntity;
      Context.OperationResult = new OperationResult.OK { ResponseResource = responseEntity };
    }

    protected void GivenAUser(string username, string password)
    {
      var provider = Resolver.Resolve<IAuthenticationProvider>() as InMemAuthenticationProvider;
      provider.Passwords[username] = password;
    }


    class InMemAuthenticationProvider : IAuthenticationProvider
    {
      public readonly Dictionary<string, string> Passwords = new Dictionary<string, string>();

      public Credentials GetByUsername(string username)
      {
        if (username == null || !Passwords.ContainsKey(username))
          return null;
        return new Credentials
        {
            Username = username,
            Password = Passwords[username],
            Roles = new string[0]
        };
      }

      public bool ValidatePassword(Credentials credentials, string suppliedPassword)
      {
        return (credentials.Password == suppliedPassword);
      }
    }

    protected TestErrorCollector Errors { get; private set; }

    [SetUp]
    protected void setup()
    {
      Host = new InMemoryHost(startup: new StartupProperties
      {
        OpenRasta =
        {
          Errors =
          {
            HandleAllExceptions = false,
            HandleCatastrophicExceptions = false
          }
        }
      });

      Pipeline = null;
      _actions = new Dictionary<Type, Func<ICommunicationContext, Task<PipelineContinuation>>>();
      var manager = Host.HostManager;

      Resolver.AddDependencyInstance(typeof(IErrorCollector), Errors = new TestErrorCollector());
      Resolver.AddDependency<IPathManager, PathManager>();

      _ambientContext = new ContextScope(new AmbientContext());
      _requestScope = Resolver.CreateRequestScope();
      manager.SetupCommunicationContext(Context = new WriteTrackingResponseCommunicationContext(InnerContext = new InMemoryCommunicationContext()));
    }

    public InMemoryCommunicationContext InnerContext { get; set; }

    [TearDown]
    protected void cleanup()
    {
      _requestScope.Dispose();
      _ambientContext.Dispose();
      Host.Close();
    }

    class SinglePipeline<T> : IPipeline, IPipelineExecutionOrder, IPipelineExecutionOrderAnd
        where T : class, IPipelineContributor
    {
      readonly Dictionary<Type, Func<ICommunicationContext, Task<PipelineContinuation>>> _actions;

      Func<ICommunicationContext, Task<PipelineContinuation>> _lastNotification;
      readonly List<Func<ICommunicationContext, Task<PipelineContinuation>>> _notifications = new List<Func<ICommunicationContext, Task<PipelineContinuation>>>();
      readonly T _contributor;

      public SinglePipeline(
          Func<T> creator,
          IDependencyResolver resolver,
          Dictionary<Type, Func<ICommunicationContext, Task<PipelineContinuation>>> actions)
      {
        ContextData = new PipelineData();
        var resolver1 = resolver;
        if (!resolver1.HasDependency(typeof(T)))
          resolver1.AddDependency<T>();
        _contributor = creator != null ? creator() : resolver.Resolve<T>();
        _actions = actions;
      }

      public IPipelineExecutionOrder And => this;

      public IEnumerable<ContributorCall> CallGraph
      {
        get => _notifications.Select(n => new ContributorCall(_contributor, n, ""));
      }

      public PipelineData ContextData { get; }

      public IList<IPipelineContributor> Contributors => new[] { _contributor };

      public bool IsInitialized { get; private set; }


      public void Initialize()
      {
        IsInitialized = true;
      }


      public IPipelineExecutionOrder Notify(Func<ICommunicationContext, PipelineContinuation> notification)
      {
        _notifications.Add(_lastNotification = env => Task.FromResult(notification(env)));

        return this;
      }

      public void Run(ICommunicationContext context)
      {
      }

      public IPipelineExecutionOrderAnd After(Type contributorType)
      {
        _actions[contributorType] = _lastNotification;
        return this;
      }

      public IPipelineExecutionOrderAnd Before(Type contributorType)
      {
        _actions[contributorType] = _lastNotification;
        return this;
      }

      public IPipelineExecutionOrder NotifyAsync(Func<ICommunicationContext, Task<PipelineContinuation>> action)
      {
        _notifications.Add(_lastNotification = action);
        return this;
      }
    }

    protected void given_first_resource_selected()
    {
      var res = UriResolver.First();

      Context.PipelineData.ResourceKey = res.ResourceKey;
      Context.PipelineData.SelectedResource = res;
    }

    protected void given_context_applicationBase(string appBasePath)
    {
      InnerContext.ApplicationBaseUri = new Uri(appBasePath, UriKind.Absolute);
    }
  }
}