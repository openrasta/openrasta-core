namespace OpenRasta.Text.Parsers
{
  public static class Abnf7230Http
  {
    public static bool IsTChar(char c)
    {
      return Abnf5234.IsAlpha(c) || Abnf5234.IsDigit(c) ||
             c == '!' || c == '#' || c == '$' || c == '%' || c == '&' || c == '\'' || c == '*' ||
             c == '+' || c == '-' || c == '.' || c == '^' || c == '_' || c == '`' || c == '|' || c == '~';
    }
  }
}