namespace OpenRasta.Plugins.Caching.Pipeline
{
  public static class BNF
  {
    public const string ETAG_C_SAFE = @"\x21\x23-\x7E";
    public const string ETAG_C = ETAG_C_SAFE + OBS_TEXT;
    public const string OBS_TEXT = @"\x80-\xFF";
  }
}