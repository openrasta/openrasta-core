using System;
using OpenRasta.Web;

namespace OpenRasta.Codecs
{
  /// <summary>
  /// Represents the result of matching a codec to method parameters.
  /// </summary>
  public class CodecMatch : IComparable<CodecMatch>
  {
    public CodecMatch(CodecRegistration codecRegistration, float score, int matchingParameters)
    {
      if (codecRegistration == null)
        throw new ArgumentNullException(nameof(codecRegistration));

      CodecRegistration = codecRegistration;
      Score = score;
      MatchingParameterCount = matchingParameters;
      MediaType = CodecRegistration.MediaType;
      WeightedScore = Score * CodecRegistration.MediaType.Quality;
    }

    MediaType MediaType { get; }

    public float WeightedScore { get; }

    public CodecRegistration CodecRegistration { get; }
    public int MatchingParameterCount { get; }
    public float Score { get; }

    public int CompareTo(CodecMatch other)
    {
      if (other?.CodecRegistration == null)
        return 1;
      if (ReferenceEquals(this, other))
        return 0;
      if (WeightedScore == other.WeightedScore)
      {
        return MatchingParameterCount == other.MatchingParameterCount
          ? MediaType.CompareTo(other.MediaType)
          : MatchingParameterCount.CompareTo(other.MatchingParameterCount);
      }

      return WeightedScore.CompareTo(other.WeightedScore);
    }
  }
}
