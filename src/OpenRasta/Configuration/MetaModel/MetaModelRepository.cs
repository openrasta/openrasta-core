using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.DI;

namespace OpenRasta.Configuration.MetaModel
{
  public class MetaModelRepository : IMetaModelRepository
  {
    readonly Func<IEnumerable<IMetaModelHandler>> _handlers;

    public MetaModelRepository(Func<IEnumerable<IMetaModelHandler>> handlers)
    {
      _handlers = handlers;
      ResourceRegistrations = new List<ResourceModel>();
      CustomRegistrations = new ArrayList();
    }

    public IList CustomRegistrations { get; }
    public IList<ResourceModel> ResourceRegistrations { get; }

    public void Process()
    {
      var  earlyHandlers = _handlers().Where(ManagesDependencies).ToList();
      ProcessHandlers(earlyHandlers);
      
      // may have registered handlers in plugins, re-running the types here
      var lateHandlers = _handlers().Where(handler => !ManagesDependencies(handler)).ToList();
      ProcessHandlers(lateHandlers);
    }

    bool ManagesDependencies(IMetaModelHandler arg) =>
      arg is DependencyFactoryHandler || arg is DependencyRegistrationMetaModelHandler;

    void ProcessHandlers(List<IMetaModelHandler> handlers)
    {
      foreach (var handler in handlers) handler.PreProcess(this);
      foreach (var handler in handlers) handler.Process(this);
    }
  }
}