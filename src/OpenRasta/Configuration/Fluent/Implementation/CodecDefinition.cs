using System;
using OpenRasta.Configuration.Fluent.Extensions;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Configuration.Fluent.Implementation
{
  public class CodecDefinition : ICodecDefinition, ICodecTarget
  {
    readonly IFluentTarget _rootTarget;
    readonly CodecModel _codecRegistration;

    public CodecDefinition(
      IFluentTarget rootTarget,
      ResourceDefinition resourceDefinition,
      Type codecType,
      object configuration)
    {
      _rootTarget = rootTarget;
      ResourceDefinition = resourceDefinition;
      _codecRegistration = new CodecModel(codecType, configuration);
      ResourceDefinition.Resource.Codecs.Add(_codecRegistration);
    }

    public ICodecParentDefinition And => ResourceDefinition;

    public ResourceDefinition ResourceDefinition { get; set; }

    public ICodecWithMediaTypeDefinition ForMediaType(MediaType mediaType)
    {
      var model = new MediaTypeModel {MediaType = mediaType};
      _codecRegistration.MediaTypes.Add(model);

      return new CodecMediaTypeDefinition(this, model);
    }

    public IMetaModelRepository Repository => _rootTarget.Repository;

    public ITypeSystem TypeSystem => _rootTarget.TypeSystem;

    public ResourceModel Resource => ResourceDefinition.Resource;

    public CodecModel Codec => _codecRegistration;
  }
}