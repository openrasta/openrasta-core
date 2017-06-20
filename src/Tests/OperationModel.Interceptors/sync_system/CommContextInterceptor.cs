using System;
using System.Collections.Generic;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.Web;

namespace Tests.OperationModel.Interceptors.sync_system
{
  public class CommContextInterceptor : IOperationInterceptor
  {
    public ICommunicationContext Context { get; }
    public bool Called { get; set; }

    public CommContextInterceptor(ICommunicationContext context)
    {
      Context = context;
    }

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