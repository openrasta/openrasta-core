using System.Diagnostics.CodeAnalysis;

namespace OpenRasta.Plugins.Caching.Pipeline
{
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
  static class AbnfRegex
  {
    public const string ETAG_C_SAFE = @"\x21\x23-\x7E";
    public const string ETAG_C = ETAG_C_SAFE + OBS_TEXT;
    public const string OBS_TEXT = @"\x80-\xFF";
  }
}