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
    public static void LegacyShouldCompleteSuccessfully(this Action codeToExecute)
    {
      codeToExecute();
    }


    public static void LegacyShouldContain<T, U>(this IDictionary<T, U> dic, T key, U value)
    {
      dic.ShouldContainKey(key);
      dic[key].ShouldBe(value);
    }

    public static T LegacyShouldBe<T>(this T valueToAnalyse, T expectedValue)
    {
      valueToAnalyse.ShouldBe(expectedValue);
      return valueToAnalyse;
    }

    public static IEnumerable<T> LegacyShouldContain<T>(this IEnumerable<T> list, T expected)
    {
      list.ShouldContain(item => ((Func<T, T, bool>) ((t, t2) => t.Equals(t2)))(item, expected));
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

    public static T legacyShouldNotBeNull<T>(this T obj) where T : class
    {
      obj.ShouldNotBeNull();
      return obj;
    }
  }
}