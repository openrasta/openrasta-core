using System;
using System.Collections.Generic;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Interceptors;

namespace Tests.OperationModel.Interceptors.sync_system
{
  public class SystemAttribute : IOperationInterceptor
  {
    public IMyService Service { get; }

    public SystemAttribute(IMyService service)
    {
      Service = service;
    }

    public bool Called { get; set; }

    public bool BeforeExecute(IOperation operation)
    {
      Called = true;
      return true;
    }

    public Func<IEnumerable<OutputMember>> RewriteOperation(Func<IEnumerable<OutputMember>> operationBuilder)
    {
      return operationBuilder;
    }

    public bool AfterExecute(IOperation operation, IEnumerable<OutputMember> outputMembers)
    {
      return true;
    }
  }
}