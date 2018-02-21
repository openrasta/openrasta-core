using System;
using System.Collections.Generic;
using Shouldly;

namespace Tests.Infrastructure
{
  public static class TestExtensions
  {
    public static void ShouldHaveSameElementsAs<T, V>(this IEnumerable<T> actual,
      IEnumerable<V> expected,
      Func<T, V, bool> comparer)
    {
      int index = 0;
      using (var actualEnumerator = actual.GetEnumerator())
      using (var expectedEnumerator = expected.GetEnumerator())
      {
        while (true)
        {
          var enum1HasMoved = actualEnumerator.MoveNext();
          var enum2HasMoved = expectedEnumerator.MoveNext();
          if (!enum1HasMoved && !enum2HasMoved)
            return;
          if (enum1HasMoved != enum2HasMoved)
            return;
          
          comparer(actualEnumerator.Current, expectedEnumerator.Current)
            .ShouldBeTrue(
              $"Two elements didnt match at index {index}:\nactual:\n{actualEnumerator.Current}\nexpected:\n{expectedEnumerator.Current}");
          index++;
        }
      }
    }
  }
}