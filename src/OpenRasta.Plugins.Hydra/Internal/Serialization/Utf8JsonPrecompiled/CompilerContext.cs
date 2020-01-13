using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Configuration.MetaModel;

namespace OpenRasta.Plugins.Hydra.Internal.Serialization.Utf8JsonPrecompiled
{
  public class CompilerContext
  {
    List<ResourceModel> _recursionDefender = new List<ResourceModel>();

    public CompilerContext(IMetaModelRepository metaModel, ResourceModel resource)
    {
      MetaModel = metaModel;
      Resource = resource;
    }

    public IMetaModelRepository MetaModel { get; }
    public ResourceModel Resource { get; }

    public CompilerContext Push(ResourceModel resourceModel)
    {
      if (_recursionDefender.Any(m => m.ResourceType == resourceModel.ResourceType))
        throw new InvalidOperationException(
          $"Detected recursion, already processing {resourceModel.ResourceType?.Name}: {String.Join("->", _recursionDefender.Select(m => m.ResourceType?.Name).Where(n => n != null))}");
      return new CompilerContext(MetaModel, resourceModel)
      {
        _recursionDefender = _recursionDefender.Concat(new[]{resourceModel}).ToList()
      };
    }

    public static string ContextUri = ".hydra/context.jsonld";
  }
}