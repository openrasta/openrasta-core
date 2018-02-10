using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OpenRasta.Collections;
using OpenRasta.TypeSystem;
using OpenRasta.TypeSystem.ReflectionBased;
using OpenRasta.Web;

namespace OpenRasta.Codecs
{
  public class CodecRepository : ICodecRepository
  {
    readonly MediaTypeDictionary<CodecRegistration> _codecs = new MediaTypeDictionary<CodecRegistration>();

    public string[] RegisteredExtensions
    {
      get { return _codecs.SelectMany(reg => reg.Extensions).ToArray(); }
    }

    public void Add(CodecRegistration codecRegistration)
    {
      _codecs.Add(codecRegistration.MediaType, codecRegistration);
    }

    public void Clear()
    {
    }

    public CodecRegistration FindByExtension(IMember resourceMember, string extension)
    {
      foreach (var codecRegistration in _codecs)
      {
        var codecResourceType = codecRegistration.ResourceType;
        if (!codecRegistration.Extensions.Contains(extension, StringComparison.OrdinalIgnoreCase)) continue;
        
        if (codecRegistration.IsStrict && resourceMember.Type.CompareTo(codecResourceType) == 0)
          return codecRegistration;

        if (resourceMember.Type.CompareTo(codecResourceType) >= 0)
          return codecRegistration;
      }

      return null;
    }

    /// <exception cref="ArgumentNullException"><c>requestedMediaType</c> is null.</exception>
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public CodecMatch FindMediaTypeReader(MediaType requestedMediaType,
        IEnumerable<IMember> requiredMembers,
        IEnumerable<IMember> optionalMembers)
    {
      if (requestedMediaType == null)
        throw new ArgumentNullException("requestedMediaType");
      if (requiredMembers == null)
        throw new ArgumentNullException("requiredMembers");
      
      var codecMatches = new List<CodecMatch>();

      var readerCodecs = from codec in _codecs.Matching(requestedMediaType)
          where codec.CodecType.Implements<IMediaTypeReader>() ||
                codec.CodecType.Implements<IMediaTypeReaderAsync>() ||
                codec.CodecType.Implements(typeof(IKeyedValuesMediaTypeReader<>))
          select codec;

      foreach (var codec in readerCodecs)
      {
        float totalDistanceToRequiredParameters = 0;
        if (requiredMembers.Any())
        {
          totalDistanceToRequiredParameters = CalculateScoreFor(requiredMembers, codec);
          // ReSharper disable once CompareOfFloatsByEqualityOperator - code needs rewriting, ignore
          if (totalDistanceToRequiredParameters == -1)
            continue; // the codec cannot resolve the required parameters
        }

        var totalDistanceToOptionalParameters = 0;
        var totalOptionalParametersCompatibleWithCodec = 0;
        if (optionalMembers != null)
          foreach (var optionalType in optionalMembers)
          {
            var typeScore = CalculateDistance(optionalType, codec);
            if (typeScore <= -1) continue;
            
            totalDistanceToOptionalParameters += typeScore;
            totalOptionalParametersCompatibleWithCodec++;
          }

        var averageScore = totalDistanceToRequiredParameters + totalDistanceToOptionalParameters;

        codecMatches.Add(new CodecMatch(codec,
            averageScore,
            requiredMembers.Count() + totalOptionalParametersCompatibleWithCodec));
      }

      if (codecMatches.Count == 0)
        return null;

      codecMatches.Sort();
      codecMatches.Reverse();
      return codecMatches[0];
    }

    public IEnumerable<CodecRegistration> FindMediaTypeWriter(IMember resourceType,
        IEnumerable<MediaType> requestedMediaTypes)
    {
      var orderedMediaTypes = requestedMediaTypes.OrderByDescending(mt => mt);
      var mediaTypesByQuality = orderedMediaTypes.GroupBy(key => key.Quality);
      return mediaTypesByQuality
          .Aggregate(new List<CodecRegistration>(), AppendMediaTypeWriterFor(resourceType));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _codecs.Distinct().GetEnumerator();
    }

    IEnumerator<CodecRegistration> IEnumerable<CodecRegistration>.GetEnumerator()
    {
      return _codecs.Distinct().GetEnumerator();
    }

    static int CalculateDistance(IMember member, CodecRegistration registration)
    {
      if (registration.ResourceType == null)
        return -1;
      if (registration.IsStrict)
        return (member.Type.CompareTo(registration.ResourceType) == 0) ? 0 : -1;
      return member.Type.CompareTo(registration.ResourceType);
    }

    Func<IEnumerable<CodecRegistration>, IGrouping<float, MediaType>, IEnumerable<CodecRegistration>> AppendMediaTypeWriterFor(IMember resourceType)
    {
      return (source, mediaTypes) => source.Concat(FindMediaTypeWriterFor(mediaTypes, resourceType));
    }

    float CalculateScoreFor(IEnumerable<IMember> types, CodecRegistration registration)
    {
      float score = 0;

      foreach (var requestedType in types)
      {
        int typeComparison = CalculateDistance(requestedType.Type, registration);
        if (typeComparison == -1)
          return -1;
        float typeScore = 1f / (1f + typeComparison);
        score += typeScore;
      }

      return score;
    }

    IEnumerable<CodecRegistration> FindMediaTypeWriterFor(IEnumerable<MediaType> mediaTypes, IMember resourceType)
    {
      return from mediaType in mediaTypes
          from codec in _codecs.Matching(mediaType)
          where codec.CodecType.Implements<IMediaTypeWriter>()
                || codec.CodecType.Implements<IMediaTypeWriterAsync>()
          let match = new CodecMatch(codec, CalculateScoreFor(new[] { resourceType }, codec), int.MaxValue)
          where match.Score >= 0
          orderby match descending
          select codec;
    }
  }
}