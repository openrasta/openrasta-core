using System;
using System.Collections.Generic;

namespace OpenRasta.OperationModel.Interceptors
{
    public interface IOperationInterceptor
    {
#pragma warning disable 612,618
      bool BeforeExecute(IOperation operation);
        Func<IEnumerable<OutputMember>> RewriteOperation(Func<IEnumerable<OutputMember>> operationBuilder);
        bool AfterExecute(IOperation operation, IEnumerable<OutputMember> outputMembers);
#pragma warning restore 612,618
    }
}