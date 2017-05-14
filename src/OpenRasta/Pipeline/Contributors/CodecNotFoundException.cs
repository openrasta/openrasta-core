using System;

namespace OpenRasta.Pipeline.Contributors
{
  internal class CodecNotFoundException : Exception
  {
    public CodecNotFoundException(string message) : base(message)
    {
    }
  }
}