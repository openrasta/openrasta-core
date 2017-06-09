using OpenRasta.Codecs;

namespace OpenRasta.OperationModel
{
  public static class OperationRequestCodecExtensions
  {
    const string REQUEST_CODEC = "_REQUEST_CODEC";

    /// <summary>
    /// Gets The codec used to read the request.
    /// </summary>
    public static CodecMatch GetRequestCodec(this IOperationAsync operation)
    {
      return operation.ExtendedProperties.TryGetValue(REQUEST_CODEC, out var instance)
        ? (CodecMatch) instance
        : null;
    }

    public static void SetRequestCodec(this IOperationAsync operation, CodecMatch codecMatch)
    {
      operation.ExtendedProperties[REQUEST_CODEC] = codecMatch;
    }
  }
}