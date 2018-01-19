using OpenRasta.Configuration.Fluent.Implementation;
using OpenRasta.Configuration.Fluent.Internal;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.DI;

namespace OpenRasta.Configuration
{
  public interface IConfigurationSource
  {
    void Configure();
  }

  public class ConfigurationSourceAdapter
  {
    readonly IDependencyResolver _resolver;
    readonly IConfigurationSource _source;
    readonly IMetaModelRepository _repository;

    public ConfigurationSourceAdapter(
      IConfigurationSource source,
      IDependencyResolver resolver,
      IMetaModelRepository repository)
    {
      _source = source;
      _repository = repository;
      _resolver = resolver;
    }

    public void Process()
    {
      AsyncLocalConfigurations.Target = new FluentTarget(_resolver, _repository);
      AsyncLocalConfigurations.ConfigurationCompletion = _repository.Process;
      try
      {
        _source.Configure();
        AsyncLocalConfigurations.ConfigurationCompletion();
      }
      finally
      {
        AsyncLocalConfigurations.Target = null;
        AsyncLocalConfigurations.ConfigurationCompletion = null;
      }
    }
  }
}