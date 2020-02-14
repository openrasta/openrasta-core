using System.IO;

namespace OpenRasta.Web
{
  public interface IMultipartHttpEntity : IHttpEntity
  {
    void SwapStream(string filepath);
    void SwapStream(Stream stream);
  }
}