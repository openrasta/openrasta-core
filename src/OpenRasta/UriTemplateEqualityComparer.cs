using System.Collections.Generic;

namespace OpenRasta
{
  public class UriTemplateEquivalenceComparer : IEqualityComparer<UriTemplate>
  {
    public bool Equals(UriTemplate x, UriTemplate y)
    {
      return (x == null && y == null) || !(x == null || y == null) || x.IsEquivalentTo(y);
      // as binary logic is not the best asset of most developers, here's the translation
      // for educational purposes.
      // if x and y are null then they're equal
      // if either x or y is null, as we know they're not both null, we return !(true) which returns false which is correct
      // and finally, neither are null so return if x is equivalent to y
    }


    public int GetHashCode(UriTemplate obj)
    {
      return obj.GetHashCode();
    }
  }
}