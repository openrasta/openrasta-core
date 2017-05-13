using System;
using System.Collections.Generic;
using Shouldly;

namespace Tests.Infrastructure
{
  public static class TestExtensions
  {
    public static void ShouldHaveSameElementsAs<T, V>(this IEnumerable<T> r1,
      IEnumerable<V> r2,
      Func<T, V, bool> comparer)
    {
      using (var enumerator1 = r1.GetEnumerator())
      using (var enumerator2 = r2.GetEnumerator())
      {
        while (true)
        {
          var enum1HasMoved = enumerator1.MoveNext();
          var enum2HasMoved = enumerator2.MoveNext();
          if (!enum1HasMoved && !enum2HasMoved)
            return;
          if (enum1HasMoved != enum2HasMoved)
            return;
          comparer(enumerator1.Current, enumerator2.Current)
            .ShouldBeTrue(
              $"Two elements didnt match:\na:\n{enumerator1.Current}\nb:\n{enumerator2.Current}");
        }
      }
    }
  }
}