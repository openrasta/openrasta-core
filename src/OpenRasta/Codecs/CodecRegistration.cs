using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Collections;
using OpenRasta.Configuration.MetaModel;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Codecs
{
  public class CodecRegistration : IEquatable<CodecRegistration>
  {
    public CodecRegistration(Type codecType, object resourceKey, MediaType mediaType)
      : this(codecType, resourceKey, false, mediaType, null, null, false)
    {
    }

    public CodecRegistration(
      Type codecType,
      object resourceKey,
      bool isStrictRegistration,
      MediaType mediaType,
      IEnumerable<string> extensions,
      object codecConfiguration,
      bool isSystem)
    {
      CheckArgumentsAreValid(codecType, resourceKey, mediaType, isStrictRegistration);
      ResourceKey = resourceKey;
      MediaType = mediaType;
      CodecType = codecType;
      Extensions = new List<string>();
      if (extensions != null)
        Extensions.AddRange(extensions);
      Configuration = codecConfiguration;
      IsSystem = isSystem;
      IsStrict = isStrictRegistration;
    }

    public CodecRegistration(Type codecType, object resourceKey, bool isStrictRegistration, MediaType mediaType, ICollection<string> mediaTypeExtensions, object codecConfiguration, bool isSystem, CodecModel codecModel)
    : this(codecType, resourceKey, isStrictRegistration, mediaType, mediaTypeExtensions, codecConfiguration, isSystem)
    {
      CodecModel = codecModel;
      IsBuffered = codecModel?.IsBuffered ?? false;
    }

    public Type CodecType { get; }
    public object Configuration { get; }
    public IList<string> Extensions { get; }
    public bool IsStrict { get; }

    /// <summary>
    /// Defines if the codec is to be preserved between configuration refreshes because it is part of the
    /// OpenRasta framework.
    /// </summary>
    public bool IsSystem { get; }

    public bool IsBuffered { get; set; }

    public MediaType MediaType { get; }
    public object ResourceKey { get; }

    public IType ResourceType => ResourceKey as IType;

    public CodecModel CodecModel { get;  }

    public static IEnumerable<CodecRegistration> FromCodecType(Type codecType, ITypeSystem typeSystem)
    {
      var resourceTypeAttributes =
        codecType.GetCustomAttributes(typeof(SupportedTypeAttribute), true).Cast<SupportedTypeAttribute>();
      var mediaTypeAttributes =
        codecType.GetCustomAttributes(typeof(MediaTypeAttribute), true).Cast<MediaTypeAttribute>();
      return from resourceTypeAttribute in resourceTypeAttributes
        from mediaType in mediaTypeAttributes
        let isStrictRegistration = IsStrictRegistration(resourceTypeAttribute.Type)
        let resourceType =
          isStrictRegistration ? GetStrictType(resourceTypeAttribute.Type) : resourceTypeAttribute.Type
        select new CodecRegistration(
          codecType,
          typeSystem.FromClr(resourceType),
          isStrictRegistration,
          mediaType.MediaType,
          mediaType.Extensions,
          null,
          true);
    }

    public static CodecRegistration FromResourceType(
      Type resourceType,
      Type codecType,
      ITypeSystem typeSystem,
      MediaType mediaType,
      IEnumerable<string> extensions,
      object codecConfiguration,
      bool isSystem)
    {
      bool isStrict = false;
      if (IsStrictRegistration(resourceType))
      {
        resourceType = GetStrictType(resourceType);
        isStrict = true;
      }

      return new CodecRegistration(codecType,
        typeSystem.FromClr(resourceType),
        isStrict,
        mediaType,
        extensions,
        codecConfiguration,
        isSystem);
    }

    public static Type GetStrictType(Type registration)
    {
      return registration.GetGenericArguments()[0];
    }

    public static bool IsStrictRegistration(Type type)
    {
      return type.IsGenericType
             && type.GetGenericTypeDefinition() == typeof(Strictly<>);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != typeof(CodecRegistration)) return false;
      return Equals((CodecRegistration) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int result = ResourceKey != null ? ResourceKey.GetHashCode() : 0;
        result = (result * 397) ^ (CodecType != null ? CodecType.GetHashCode() : 0);
        result = (result * 397) ^ (MediaType != null ? MediaType.GetHashCode() : 0);
        result = (result * 397) ^ IsStrict.GetHashCode();
        result = (result * 397) ^ (Extensions != null ? Extensions.GetHashCode() : 0);
        result = (result * 397) ^ (Configuration != null ? Configuration.GetHashCode() : 0);
        result = (result * 397) ^ IsSystem.GetHashCode();
        return result;
      }
    }

    public bool Equals(CodecRegistration other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return Equals(other.ResourceKey, ResourceKey) && Equals(other.CodecType, CodecType) &&
             Equals(other.MediaType, MediaType) && other.IsStrict.Equals(IsStrict) &&
             Equals(other.Extensions, Extensions) && Equals(other.Configuration, Configuration) &&
             other.IsSystem.Equals(IsSystem);
    }

    /// <exception cref="ArgumentException">Cannot do a strict registration on resources with keys that are not types.</exception>
    /// <exception cref="ArgumentNullException"><c>mediaType</c> is null.</exception>
    static void CheckArgumentsAreValid(
      Type codecType,
      object resourceKey,
      MediaType mediaType,
      bool isStrictRegistration)
    {
      if (codecType == null)
        throw new ArgumentNullException("codecType", "codecType is null.");
      if (resourceKey == null)
        throw new ArgumentNullException("resourceKey", "resourceKey is null.");
      if (mediaType == null)
        throw new ArgumentNullException("mediaType", "mediaType is null.");
      if (resourceKey is Type)
        throw new ArgumentException("If using a type as a resourceKey, use an IType instead.", "resourceKey");
      if (isStrictRegistration && !(resourceKey is IType))
        throw new ArgumentException(
          "Cannot do a strict registration on resources with keys that are not types.", "isStrictRegistration");
    }
  }
}