#region License

/* Authors:
 *      Sebastien Lambla (seb@serialseb.com)
 * Copyright:
 *      (C) 2007-2009 Caffeine IT & naughtyProd Ltd (http://www.caffeine-it.com)
 * License:
 *      This file is distributed under the terms of the MIT License found at the end of this file.
 */


#endregion
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

namespace OpenRasta.Testing
{
  public static class SpecExtensions
  {
    public static void LegacyShouldAllBe<T>(this IEnumerable<T> values, T expected)
    {
      //values.LegacyShouldAllBe(expected);
    }

    public static T LegacyShouldBe<T>(this T valueToAnalyse, T expectedValue)
    {
      Assert.That(valueToAnalyse, Is.EqualTo(expectedValue));
      return valueToAnalyse;
    }

    public static void LegacyShouldBe<T>(this Type valueToAnalyse)
    {
      Assert.That(valueToAnalyse, Is.EqualTo(typeof(T)));
      return;
    }

    public static Uri LegacyShouldBe(this Uri actual, string expected)
    {
      var uri = new Uri(expected);
      Assert.That(actual, Is.EqualTo(uri));
      return uri;
    }

    public static TExpected LegacyShouldBeAssignableTo<TExpected>(this object obj)
    {
      if (!(obj is TExpected))
        Assert.Fail("TargetType {0} is not implementing {1}.", obj.GetType().Name, typeof(TExpected).Name);
      return (TExpected) obj;
    }

    public static void ShouldBeEmpty<T>(this IEnumerable<T> values)
    {
      values.LegacyShouldHaveCountOf(0);
    }

    public static bool LegacyShouldBeFalse(this bool value)
    {
      Assert.That(value, Is.False);
      return false;
    }

    public static void LegacyShouldBeGreaterThan<T>(this T actual, T expected) where T : IComparable
    {
      Assert.That(actual, Is.GreaterThan(expected));
    }

    public static void LegacyShouldBeNull<T>(this T obj)
    {
      Assert.IsNull(obj);
    }

    public static TExpected LegacyShouldBeOfType<TExpected>(this object obj)
    {
      Assert.That(obj, Is.InstanceOf<TExpected>());
      return obj == null ? default(TExpected) : (TExpected) obj;
    }

    public static void LegacyShouldBeTheSameInstanceAs(this object actual, object expected)
    {
      Assert.That(actual, Is.SameAs(expected));
    }

    public static void LegacyShouldBeTrue(this bool value)
    {
      Assert.That(value, Is.True);
    }

    public static void LegacyShouldCompleteSuccessfully(this Action codeToExecute)
    {
      codeToExecute();
    }

    public static IDictionary<T, U> LegacyShouldContain<T, U>(this IDictionary<T, U> dic, T key, U value)
    {
      dic[key].LegacyShouldBe(value);
      return dic;
    }

    public static IEnumerable<T> LegacyShouldContain<T>(this IEnumerable<T> list, T expected)
    {
      LegacyShouldContain(list, expected, (t, t2) => t.Equals(t2));
      return list;
    }

    public static void LegacyShouldContain<T>(this IEnumerable<T> list, T expected, Func<T, T, bool> match)
    {
      if (list.Any(t => match(t, expected)))
      {
        return;
      }

      Assert.Fail("Looking for element {0} but didn't find any.", expected);
      return;
    }

    public static string LegacyShouldContain(this string baseString, string textToFind)
    {
      if (baseString.IndexOf(textToFind, StringComparison.Ordinal) == -1)
        Assert.Fail("text '{0}' not found in '{1}'", textToFind, baseString);
      return baseString;
    }

    public static IEnumerable<T> LegacyShouldHaveCountOf<T>(this IEnumerable<T> values, int count)
    {
      values.legacyShouldNotBeNull().Count().LegacyShouldBe(count);
      return values;
    }

    public static bool LegacyShouldHaveSameElementsAs(this NameValueCollection expectedResult,
      NameValueCollection actualResult)
    {
      Assert.AreEqual(expectedResult.Count, actualResult.Count);
      foreach (string key in expectedResult.Keys)
      {
        Assert.AreEqual(expectedResult[key], actualResult[key]);
        if (expectedResult[key] != actualResult[key])
          return false;
      }
      return true;
    }

    public static void LegacyShouldHaveSameElementsAs<T>(this IEnumerable<T> r1, IEnumerable<T> r2)
    {
      var enumerator1 = r1.GetEnumerator();
      var enumerator2 = r2.GetEnumerator();
      bool moveNext1 = false, moveNext2 = false;
      while (((moveNext1 = enumerator1.MoveNext()) & (moveNext2 = enumerator2.MoveNext()))
             && moveNext1 == moveNext2)
        Assert.AreEqual(enumerator1.Current, enumerator2.Current);
      if (moveNext1 != moveNext2)
        Assert.Fail("The two enumerables didn't have the same number of elements.");
      enumerator1.Dispose();
      enumerator2.Dispose();
    }

    public static T LegacyShouldNotBe<T>(this T valueToAnalyse, T expectedValue)
    {
      Assert.That(valueToAnalyse, Is.Not.EqualTo(expectedValue));
      return valueToAnalyse;
    }

    public static T legacyShouldNotBeNull<T>(this T obj) where T : class
    {
      Assert.IsNotNull(obj);
      return obj;
    }

    public static T legacyShouldNotBeNull<T>(this T? obj) where T : struct
    {
      Assert.True(obj.HasValue);
      return obj.Value;
    }

    public static void LegacyShouldNotBeTheSameInstanceAs(this object actual, object expected)
    {
      Assert.That(actual, Is.Not.SameAs(expected));
    }

    public static void LegacyShouldNotContain(this string baseString, string textToFind)
    {
      if (baseString.IndexOf(textToFind, StringComparison.Ordinal) != -1)
        Assert.Fail("text '{0}' found in '{1}'", textToFind, baseString);
    }

    public static T LegacyShouldThrow<T>(this Func<Task> codeToExecute) where T : Exception
    {
      try
      {
        codeToExecute().GetAwaiter().GetResult();
      }
      catch (Exception e)
      {
        if (!(e is T))
          Assert.Fail("Expected exception of type \"{0}\" but got \"{1}\" instead.",
            typeof(T).Name,
            e.GetType().Name);
        else
          return (T) e;
      }

      Assert.Fail("Expected an exception of type \"{0}\" but none were thrown.", typeof(T).Name);
      return null; // this never happens as Fail will throw...
    }

    public static T LegacyShouldThrow<T>(this Action codeToExecute) where T : Exception
    {
      try
      {
        codeToExecute();
      }
      catch (Exception e)
      {
        if (!(e is T))
          Assert.Fail("Expected exception of type \"{0}\" but got \"{1}\" instead.",
            typeof(T).Name,
            e.GetType().Name);
        else
          return (T) e;
      }

      Assert.Fail("Expected an exception of type \"{0}\" but none were thrown.", typeof(T).Name);
      return null; // this never happens as Fail will throw...
    }
  }
}

#region Full license

// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#endregion