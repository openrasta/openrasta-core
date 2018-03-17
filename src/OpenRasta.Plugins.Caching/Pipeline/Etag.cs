using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using OpenRasta.Web;

namespace OpenRasta.Plugins.Caching.Pipeline
{
  public static class Etag
  {
    public static string StrongEtag(string partialEtag, MediaType mediaType = null, Encoding charset = null,
      CultureInfo language = null, string encoding = null)
    {
      // TODO: Once the design has settled, make the etag generation strategy swappable
      return string.Format(@"""{0}:{1}:{2}:{3}:{4}""", mediaType, charset, language, encoding,
        SanitizeEtag(partialEtag));
    }

    static string SanitizeEtag(string partialEtag)
    {
      return Regex.Replace(partialEtag, string.Format(@"[^{0}]", BNF.ETAG_C_SAFE), "_");
    }
  }
}