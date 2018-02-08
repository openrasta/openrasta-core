using OpenRasta.OperationModel;
using OpenRasta.OperationModel.MethodBased;

namespace OpenRasta.Configuration.MetaModel.Handlers
{
  public class OperationModelCreator : IMetaModelHandler
  {
    public OperationModelCreator()
    {
    }
    public void PreProcess(IMetaModelRepository repository)
    {
      foreach (var resourceModel in repository.ResourceRegistrations)
        CreateOperationsForModel(resourceModel);
    }

    void CreateOperationsForModel(ResourceModel resourceModel)
    {
//      foreach()
//      resourceModel.Handlers.
    }

    public void Process(IMetaModelRepository repository)
    {
    }
  }
}