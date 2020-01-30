using OpenRasta.Configuration.Fluent.Extensions;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.TypeSystem;

namespace OpenRasta.Configuration.Fluent.Implementation
{
  public abstract class TargetWrapper : IResourceTarget
  {
    readonly ResourceDefinition _target;

    protected TargetWrapper(ResourceDefinition target)
    {
      _target = target;
    }

    public ResourceModel Resource => _target.Resource;

    public ITypeSystem TypeSystem => _target.TypeSystem;

    public IMetaModelRepository Repository => _target.Repository;
  }
}