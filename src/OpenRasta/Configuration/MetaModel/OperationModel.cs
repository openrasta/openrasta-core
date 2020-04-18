using System;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.MethodBased;

namespace OpenRasta.Configuration.MetaModel
{
  public class OperationModel
  {
    public string HttpMethod { get; set; }
    public string Name { get; set; }
    
    public Func<IOperationAsync> Factory { get; set; }
    public OperationDescriptor Descriptor { get; set; }
  }
}