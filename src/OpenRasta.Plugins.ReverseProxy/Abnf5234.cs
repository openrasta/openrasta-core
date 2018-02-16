namespace OpenRasta.Plugins.ReverseProxy
{
  static class Abnf5234
  {
    public static bool IsAlpha(char c)
    {
      return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
    }

    public static bool IsDigit(char c)
    {
      return c >= '0' && c <= '9';
    }
  }
}