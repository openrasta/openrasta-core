using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenRasta.Collections;

namespace OpenRasta
{
    public static class FuncExtensions
    {
        public static Func<T,T> Chain<T>(this IEnumerable<Func<T,T>> functions)
        {
          var enumerable = functions as Func<T, T>[] ?? functions.ToArray();
          return enumerable
            .Skip(1)
            .Aggregate(enumerable[0], (func1, func2) => t => func1(func2(t)));
        }
    }
}