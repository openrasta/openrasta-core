// ReSharper disable UnusedTypeParameter

using System;
using OpenRasta.Pipeline;

namespace OpenRasta.OperationModel
{
  [Obsolete]
    public interface IOperationProcessor<T> : IOperationProcessor
        where T : IPipelineContributor
    {
    }
}

// ReSharper restore UnusedTypeParameter