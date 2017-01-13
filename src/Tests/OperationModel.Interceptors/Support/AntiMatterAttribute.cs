using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Interceptors;

namespace Tests.OperationModel.Interceptors.Support
{
  public class AntiMatterAttribute : Attribute, IOperationInterceptorAsync
  {
    public Func<IOperationAsync, Task<IEnumerable<OutputMember>>> Compose(Func<IOperationAsync, Task<IEnumerable<OutputMember>>> next)
    {
      return operation => Task.FromResult<IEnumerable<OutputMember>>(new []{new OutputMember{Value = true}});
    }
  }
}