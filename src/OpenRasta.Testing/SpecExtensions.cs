using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Collections.Specialized;
using Shouldly;

namespace OpenRasta.Testing
{
  public static class SpecExtensions
  {
    public static IEnumerable<T> LegacyShouldContain<T>(this IEnumerable<T> list, T expected)
    {
      list.ShouldContain(expected);
      return list;
    }

    public static string LegacyShouldContain(this string baseString, string textToFind)
    {
      baseString.ShouldContain(textToFind, Case.Sensitive);
      return baseString;
    }

    public static IEnumerable<T> LegacyShouldHaveCountOf<T>(this IEnumerable<T> values, int count)
    {
      values.Count().ShouldBe(count);
      return values;
    }
  }
}