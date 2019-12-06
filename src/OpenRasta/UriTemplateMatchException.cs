using System;
using System.Runtime.Serialization;

namespace OpenRasta
{
  public class UriTemplateMatchException : SystemException
  {
    public UriTemplateMatchException() : base()
    {
    }

    public UriTemplateMatchException(string message) : base(message)
    {
    }

    public UriTemplateMatchException(string message, Exception innerException) : base(message, innerException)
    {
    }
    
    protected UriTemplateMatchException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
  }
}