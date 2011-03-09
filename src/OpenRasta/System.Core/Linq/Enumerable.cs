JAAAAAAAAAAAAAAAMONERO
JAMONERO
JAMO
﻿#region License
/* Authors:
 *      Sebastien Lambla (seb@serialseb.com)
 * Copyright:
 *      (C) 2007-2009 Caffeine IT & naughtyProd Ltd (http://www.caffeine-it.com)
 * License:
 *      This file is distributed under the terms of the MIT License found at the end of this file.
 */
#endregion
//
// Enumerable.cs
//
// Authors:
//  Marek Safar (marek.safar@gmail.com)
//  Antonello Provenzano  <antonello@deveel.com>
//  Alejandro Serrano "Serras" (trupill@yahoo.es)
//  Jb Evain (jbevain@novell.com)
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

// precious: http://www.hookedonlinq.com
#if __OW_PROFILE_net20 || __OW_PROFILE_net30__
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Linq
{
    public static class Enumerable
    {
        enum Fallback
        {
            Default,
            Throw
        }

        class PredicateOf<T>
        {
            public static readonly Func<T, bool> Always = (t) => true;
        }

        class Function<T>
        {
            public static readonly Func<T, T> Identity = (t) => t;
        }

        #region Aggregate

        public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
        {
            Check.SourceAndFunc(source, func);

            // custom foreach so that we can efficiently throw an exception
            // if zero elements and treat the first element differently
            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    throw new InvalidOperationException("No elements in source list");

                TSource folded = enumerator.Current;
                while (enumerator.MoveNext())
                    folded = func(folded, enumerator.Current);
                return folded;
            }
        }

        public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source,
            TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
        {
            Check.SourceAndFunc(source, func);

            TAccumulate folded = seed;
            foreach (TSource element in source)
                folded = func(folded, element);

            return folded;
        }

        public static TResult Aggregate<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
        {
            Check.SourceAndFunc(source, func);
            if (resultSelector == null)
                throw new ArgumentNullException("resultSelector");

            var result = seed;
            foreach (var e in source)
                result = func(result, e);

            return resultSelector(result);
        }

        #endregion

        #region All

        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Check.SourceAndPredicate(source, predicate);

            foreach (var element in source)
                if (!predicate(element))
                    return false;

            return true;
        }

        #endregion

        #region Any

        public static bool Any<TSource>(this IEnumerable<TSource> source)
        {
            Check.Source(source);

            var collection = source as ICollection<TSource>;
            if (collection != null)
                return collection.Count > 0;

            using (var enumerator = source.GetEnumerator())
                return enumerator.MoveNext();
        }

        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Check.SourceAndPredicate(source, predicate);

            foreach (TSource element in source)
                if (predicate(element))
                    return true;

            return false;
        }

        #endregion

        #region AsEnumerable

        public static IEnumerable<TSource> AsEnumerable<TSource>(this IEnumerable<TSource> source)
        {
            return source;
        }

        #endregion

        #region Average

        public static double Average(this IEnumerable<int> source)
        {
            return Average<int, long, double>(source, (a, b) => a + b, (a, b) => (double)a / (double)b);
        }

        public static double Average(this IEnumerable<long> source)
        {
            return Average<long, long, double>(source, (a, b) => a + b, (a, b) => (double)a / (double)b);
        }

        public static double Average(this IEnumerable<double> source)
        {
            return Average<double, double, double>(source, (a, b) => a + b, (a, b) => a / b);
        }

        public static float Average(this IEnumerable<float> source)
        {
            return Average<float, double, float>(source, (a, b) => a + b, (a, b) => (float)a / (float)b);
        }

        public static decimal Average(this IEnumerable<decimal> source)
        {
            return Average<decimal, decimal, decimal>(source, (a, b) => a + b, (a, b) => a / b);
        }

        static TResult Average<TElement, TAggregate, TResult>(this IEnumerable<TElement> source,
            Func<TAggregate, TElement, TAggregate> func, Func<TAggregate, long, TResult> result)
            where TElement : struct
            where TAggregate : struct
            where TResult : struct
        {
            Check.Source(source);

            var total = default(TAggregate);
            long counter = 0;
            foreach (var element in source)
            {
                total = func(total, element);
                ++counter;
            }

            if (counter == 0)
                throw new InvalidOperationException();

            return result(total, counter);
        }

        static TResult? AverageNullable<TElement, TAggregate, TResult>(this IEnumerable<TElement?> source,
            Func<TAggregate, TElement, TAggregate> func, Func<TAggregate, long, TResult> result)
            where TElement : struct
            where TAggregate : struct
            where TResult : struct
        {
            Check.Source(source);

            var total = default(TAggregate);
            long counter = 0;
            foreach (var element in source)
            {
                if (!element.HasValue)
                    continue;

                total = func(total, element.Value);
                counter++;
            }

            if (counter == 0)
                return null;

            return new TResult?(result(total, counter));
        }

        public static double? Average(this IEnumerable<int?> source)
        {
            Check.Source(source);

            return source.AverageNullable<int, long, double>((a, b) => a + b, (a, b) => (double)a / (double)b);
        }

        public static double? Average(this IEnumerable<long?> source)
        {
            Check.Source(source);

            return source.AverageNullable<long, long, double>((a, b) => a + b, (a, b) => (double)a / b);
        }

        public static double? Average(this IEnumerable<double?> source)
        {
            Check.Source(source);

            return source.AverageNullable<double, double, double>((a, b) => a + b, (a, b) => a / b);
        }

        public static decimal? Average(this IEnumerable<decimal?> source)
        {
            Check.Source(source);

            return source.AverageNullable<decimal, decimal, decimal>((a, b) => a + b, (a, b) => a / b);
        }

        public static float? Average(this IEnumerable<float?> source)
        {
            Check.Source(source);

            return source.AverageNullable<float, double, float>((a, b) => a + b, (a, b) => (float)a / (float)b);
        }

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            Check.SourceAndSelector(source, selector);

            return source.Select(selector).Average<int, long, double>((a, b) => a + b, (a, b) => (double)a / (double)b);
        }

        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
        {
            Check.SourceAndSelector(source, selector);

            return source.Select(selector).AverageNullable<int, long, double>((a, b) => a + b, (a, b) => (double)a / (double)b);
        }

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
        {
            Check.SourceAndSelector(source, selector);

            return source.Select(selector).Average<long, long, double>((a, b) => a + b, (a, b) => (double)a / (double)b);
        }

        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
        {
            Check.SourceAndSelector(source, selector);

            return source.Select(selector).AverageNullable<long, long, double>((a, b) => a + b, (a, b) => (double)a / (double)b);
        }

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            Check.SourceAndSelector(source, selector);

            return source.Select(selector).Average<double, double, double>((a, b) => a + b, (a, b) => a / b);
        }

        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
        {
            Check.SourceAndSelector(source, selector);

            return source.Select(selector).AverageNullable<double, double, double>((a, b) => a + b, (a, b) => a / b);
        }

        public static float Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
        {
            Check.SourceAndSelector(source, selector);

            return source.Select(selector).Average<float, double, float>((a, b) => a + b, (a, b) => (float)a / (float)b);
        }

        public static float? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
        {
            Check.SourceAndSelector(source, selector);

            return source.Select(selector).AverageNullable<float, double, float>((a, b) => a + b, (a, b) => (float)a / (float)b);
        }

        public static decimal Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
        {
            Check.SourceAndSelector(source, selector);

            return source.Select(selector).Average<decimal, decimal, decimal>((a, b) => a + b, (a, b) => a / b);
        }

        public static decimal? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
        {
            Check.SourceAndSelector(source, selector);

            return source.Select(selector).AverageNullable<decimal, decimal, decimal>((a, b) => a + b, (a, b) => a / b);
        }

        #endregion

        #region Cast

        public static IEnumerable<TResult> Cast<TResult>(this IEnumerable source)
        {
            Check.Source(source);

            return CreateCastIterator<TResult>(source);
        }

        static IEnumerable<TResult> CreateCastIterator<TResult>(IEnumerable source)
        {
            foreach (TResult element in source)
                yield return element;
        }

        #endregion

        #region Concat

        public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            Check.FirstAndSecond(first, second);

            return CreateConcatIterator(first, second);
        }

        static IEnumerable<TSource> CreateConcatIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            foreach (TSource element in first)
                yield return element;
            foreach (TSource element in second)
                yield return element;
        }

        #endregion

        #region Contains

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value)
        {
            var collection = source as ICollection<TSource>;
            if (collection != null)
                return collection.Contains(value);

            return Contains<TSource>(source, value, null);
        }

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
        {
            Check.Source(source);

            if (comparer == null)
                comparer = EqualityComparer<TSource>.Default;

            foreach (var element in source)
                if (comparer.Equals(element, value))
                    return true;

            return false;
        }
        #endregion

        #region Count

        public static int Count<TSource>(this IEnumerable<TSource> source)
        {
            Check.Source(source);

            var collection = source as ICollection<TSource>;
            if (collection != null)
                return collection.Count;

            int counter = 0;
            using (var enumerator = source.GetEnumerator())
                while (enumerator.MoveNext())
                    counter++;

            return counter;
        }

        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> selector)
        {
            Check.SourceAndSelector(source, selector);

            int counter = 0;
            foreach (var element in source)
                if (selector(element))
                    counter++;

            return counter;
        }

        #endregion

        #region DefaultIfEmpty

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source)
        {
            return DefaultIfEmpty(source, default(TSource));
        }

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source, TSource defaultValue)
        {
            Check.Source(source);

            return CreateDefaultIfEmptyIterator(source, defaultValue);
        }

        static IEnumerable<TSource> CreateDefaultIfEmptyIterator<TSource>(IEnumerable<TSource> source, TSource defaultValue)
        {
            bool empty = true;
            foreach (TSource item in source)
            {
                empty = false;
                yield return item;
            }

            if (empty)
                yield return defaultValue;
        }

        #endregion

        #region Distinct

        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source)
        {
            return Distinct<TSource>(source, null);
        }

        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
        {
            Check.Source(source);

            if (comparer == null)
                comparer = EqualityComparer<TSource>.Default;

            return CreateDistinctIterator(source, comparer);
        }

        static IEnumerable<TSource> CreateDistinctIterator<TSource>(IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
        {
            var items = new HashSet<TSource>(comparer);
            foreach (var element in source)
            {
                if (!items.Contains(element))
                {
                    items.Add(element);
                    yield return element;
                }
            }
        }

        #endregion

        #region ElementAt

        static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index, Fallback fallback)
        {
            long counter = 0L;

            foreach (var element in source)
            {
                if (index == counter++)
                    return element;
            }

            if (fallback == Fallback.Throw)
                throw new ArgumentOutOfRangeException();

            return default(TSource);
        }

        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index)
        {
            Check.Source(source);

            if (index < 0)
                throw new ArgumentOutOfRangeException();

            var list = source as IList<TSource>;
            if (list != null)
                return list[index];

            return source.ElementAt(index, Fallback.Throw);
        }

        #endregion

        #region ElementAtOrDefault

        public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index)
        {
            Check.Source(source);

            if (index < 0)
                return default(TSource);

            var list = source as IList<TSource>;
            if (list != null)
                return index < list.Count ? list[index] : default(TSource);

            return source.ElementAt(index, Fallback.Default);
        }

        #endregion

        #region Empty

        public static IEnumerable<TResult> Empty<TResult>()
        {
            return new TResult[0];
        }

        #endregion

        #region Except

        public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            return Except(first, second, null);
        }

        public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            Check.FirstAndSecond(first, second);

            if (comparer == null)
                comparer = EqualityComparer<TSource>.Default;

            return CreateExceptIterator(first, second, comparer);
        }

        static IEnumerable<TSource> CreateExceptIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            var items = new HashSet<TSource>(second, comparer);
            foreach (var element in first)
            {
                if (!items.Contains(element, comparer))
                    yield return element;
            }
        }

        #endregion

        #region First

        static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, Fallback fallback)
        {
            foreach (var element in source)
                if (predicate(element))
                    return element;

            if (fallback == Fallback.Throw)
                throw new InvalidOperationException();

            return default(TSource);
        }

        public static TSource First<TSource>(this IEnumerable<TSource> source)
        {
            Check.Source(source);

            var list = source as IList<TSource>;
            if (list != null)
            {
                if (list.Count != 0)
                    return list[0];

                throw new InvalidOperationException();
            }
            else
            {
                using (var enumerator = source.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                        return enumerator.Current;
                }
            }

            throw new InvalidOperationException();
        }

        public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Check.SourceAndPredicate(source, predicate);

            return source.First(predicate, Fallback.Throw);
        }

        #endregion

        #region FirstOrDefault

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            Check.Source(source);

            return source.First(PredicateOf<TSource>.Always, Fallback.Default);
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Check.SourceAndPredicate(source, predicate);

            return source.First(predicate, Fallback.Default);
        }

        #endregion

        #region GroupBy

        private static List<T> ContainsGroup<K, T>(
                Dictionary<K, List<T>> items, K key, IEqualityComparer<K> comparer)
        {
            IEqualityComparer<K> comparerInUse = (comparer ?? EqualityComparer<K>.Default);
            foreach (KeyValuePair<K, List<T>> value in items)
            {
                if (comparerInUse.Equals(value.Key, key))
                    return value.Value;
            }
            return null;
        }

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            return GroupBy<TSource, TKey>(source, keySelector, null);
        }

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            Check.SourceAndKeySelector(source, keySelector);

            return CreateGroupByIterator(source, keySelector, comparer);
        }

        static IEnumerable<IGrouping<TKey, TSource>> CreateGroupByIterator<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            Dictionary<TKey, List<TSource>> groups = new Dictionary<TKey, List<TSource>>();
            List<TSource> nullList = new List<TSource>();
            int counter = 0;
            int nullCounter = -1;

            foreach (TSource element in source)
            {
                TKey key = keySelector(element);
                if (key == null)
                {
                    nullList.Add(element);
                    if (nullCounter == -1)
                    {
                        nullCounter = counter;
                        counter++;
                    }
                }
                else
                {
                    List<TSource> group = ContainsGroup<TKey, TSource>(groups, key, comparer);
                    if (group == null)
                    {
                        group = new List<TSource>();
                        groups.Add(key, group);
                        counter++;
                    }
                    group.Add(element);
                }
            }

            counter = 0;
            foreach (KeyValuePair<TKey, List<TSource>> group in groups)
            {
                if (counter == nullCounter)
                {
                    Grouping<TKey, TSource> nullGroup = new Grouping<TKey, TSource>(default(TKey), nullList);
                    yield return nullGroup;
                    counter++;
                }
                Grouping<TKey, TSource> grouping = new Grouping<TKey, TSource>(group.Key, group.Value);
                yield return grouping;
                counter++;
            }
        }

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return GroupBy<TSource, TKey, TElement>(source, keySelector, elementSelector, null);
        }

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            Check.SourceAndKeyElementSelectors(source, keySelector, elementSelector);

            Dictionary<TKey, List<TElement>> groups = new Dictionary<TKey, List<TElement>>();
            List<TElement> nullList = new List<TElement>();
            int counter = 0;
            int nullCounter = -1;

            foreach (TSource item in source)
            {
                TKey key = keySelector(item);
                TElement element = elementSelector(item);
                if (key == null)
                {
                    nullList.Add(element);
                    if (nullCounter == -1)
                    {
                        nullCounter = counter;
                        counter++;
                    }
                }
                else
                {
                    List<TElement> group = ContainsGroup<TKey, TElement>(groups, key, comparer);
                    if (group == null)
                    {
                        group = new List<TElement>();
                        groups.Add(key, group);
                        counter++;
                    }
                    group.Add(element);
                }
            }

            counter = 0;
            foreach (KeyValuePair<TKey, List<TElement>> group in groups)
            {
                if (counter == nullCounter)
                {
                    Grouping<TKey, TElement> nullGroup = new Grouping<TKey, TElement>(default(TKey), nullList);
                    yield return nullGroup;
                    counter++;
                }
                Grouping<TKey, TElement> grouping = new Grouping<TKey, TElement>(group.Key, group.Value);
                yield return grouping;
                counter++;
            }
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector,
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
        {
            return GroupBy(source, keySelector, elementSelector, resultSelector, null);
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector,
            Func<TKey, IEnumerable<TElement>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer)
        {
            IEnumerable<IGrouping<TKey, TElement>> groups = GroupBy<TSource, TKey, TElement>(
                source, keySelector, elementSelector, comparer);

            foreach (IGrouping<TKey, TElement> group in groups)
                yield return resultSelector(group.Key, group);
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey, IEnumerable<TSource>, TResult> resultSelector)
        {
            return GroupBy(source, keySelector, resultSelector, null);
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey, IEnumerable<TSource>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer)
        {
            IEnumerable<IGrouping<TKey, TSource>> groups = GroupBy<TSource, TKey>(source, keySelector, comparer);

            foreach (IGrouping<TKey, TSource> group in groups)
                yield return resultSelector(group.Key, group);
        }

        #endregion

        # region GroupJoin

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
        {
            return GroupJoin(outer, inner, outerKeySelector, innerKeySelector, resultSelector, null);
        }

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer)
        {
            Check.JoinSelectors(outer, inner, outerKeySelector, innerKeySelector, resultSelector);

            if (comparer == null)
                comparer = EqualityComparer<TKey>.Default;

            return CreateGroupJoinIterator(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
        }

        static IEnumerable<TResult> CreateGroupJoinIterator<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
            IEqualityComparer<TKey> comparer)
        {
            ILookup<TKey, TInner> innerKeys = ToLookup<TInner, TKey>(inner, innerKeySelector, comparer);
            /*Dictionary<K, List<U>> innerKeys = new Dictionary<K, List<U>> ();
            foreach (U element in inner)
            {
                    K innerKey = innerKeySelector (element);
                    if (!innerKeys.ContainsKey (innerKey))
                            innerKeys.Add (innerKey, new List<U> ());
                    innerKeys[innerKey].Add (element);
            }*/

            foreach (TOuter element in outer)
            {
                TKey outerKey = outerKeySelector(element);
                if (innerKeys.Contains(outerKey))
                    yield return resultSelector(element, innerKeys[outerKey]);
                else
                    yield return resultSelector(element, Empty<TInner>());
            }
        }

        #endregion

        #region Intersect

        public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            return Intersect(first, second, null);
        }

        public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            Check.FirstAndSecond(first, second);

            if (comparer == null)
                comparer = EqualityComparer<TSource>.Default;

            return CreateIntersectIterator(first, second, comparer);
        }

        static IEnumerable<TSource> CreateIntersectIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            var items = new HashSet<TSource>(second, comparer);
            foreach (TSource element in first)
            {
                if (items.Contains(element))
                    yield return element;
            }
        }

        #endregion

        # region Join

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            Check.JoinSelectors(outer, inner, outerKeySelector, innerKeySelector, resultSelector);

            if (comparer == null)
                comparer = EqualityComparer<TKey>.Default;

            return CreateJoinIterator(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
        }

        static IEnumerable<TResult> CreateJoinIterator<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            ILookup<TKey, TInner> innerKeys = ToLookup<TInner, TKey>(inner, innerKeySelector, comparer);
            /*Dictionary<K, List<U>> innerKeys = new Dictionary<K, List<U>> ();
            foreach (U element in inner)
            {
                    K innerKey = innerKeySelector (element);
                    if (!innerKeys.ContainsKey (innerKey))
                            innerKeys.Add (innerKey, new List<U> ());
                    innerKeys[innerKey].Add (element);
            }*/

            foreach (TOuter element in outer)
            {
                TKey outerKey = outerKeySelector(element);
                if (innerKeys.Contains(outerKey))
                {
                    foreach (TInner innerElement in innerKeys[outerKey])
                        yield return resultSelector(element, innerElement);
                }
            }
        }

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector)
        {
            return outer.Join(inner, outerKeySelector, innerKeySelector, resultSelector, null);
        }

        #endregion

        #region Last

        static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, Fallback fallback)
        {
            var empty = true;
            var item = default(TSource);

            foreach (var element in source)
            {
                if (!predicate(element))
                    continue;

                item = element;
                empty = false;
            }

            if (!empty)
                return item;

            if (fallback == Fallback.Throw)
                throw new InvalidOperationException();

            return item;
        }

        public static TSource Last<TSource>(this IEnumerable<TSource> source)
        {
            Check.Source(source);

            var collection = source as ICollection<TSource>;
            if (collection != null && collection.Count == 0)
                throw new InvalidOperationException();

            var list = source as IList<TSource>;
            if (list != null)
                return list[list.Count - 1];

            return source.Last(PredicateOf<TSource>.Always, Fallback.Throw);
        }

        public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Check.SourceAndPredicate(source, predicate);

            return source.Last(predicate, Fallback.Throw);
        }

        #endregion

        #region LastOrDefault

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            Check.Source(source);

            var list = source as IList<TSource>;
            if (list != null)
                return list.Count > 0 ? list[list.Count - 1] : default(TSource);

            return source.Last(PredicateOf<TSource>.Always, Fallback.Default);
        }

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Check.SourceAndPredicate(source, predicate);

            return source.Last(predicate, Fallback.Default);
        }

        #endregion

        #region LongCount

        public static long LongCount<TSource>(this IEnumerable<TSource> source)
        {
            Check.Source(source);

#if !NET_2_1
            var array = source as TSource[];
            if (array != null)
                return array.LongLength;
#endif

            long counter = 0;
            using (var enumerator = source.GetEnumerator())
                while (enumerator.MoveNext())
                    counter++;

            return counter;
        }

        public static long LongCount<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> selector)
        {
            Check.SourceAndSelector(source, selector);

            long counter = 0;
            foreach (TSource element in source)
                if (selector(element))
                    counter++;

            return counter;
        }

        #endregion

        #region Max

        public static int Max(this IEnumerable<int> source)
        {
            Check.Source(source);

            return Iterate(source, int.MinValue, (a, b) => Math.Max(a, b));
        }

        public static long Max(this IEnumerable<long> source)
        {
            Check.Source(source);

            return Iterate(source, long.MinValue, (a, b) => Math.Max(a, b));
        }

        public static double Max(this IEnumerable<double> source)
        {
            Check.Source(source);

            return Iterate(source, double.MinValue, (a, b) => Math.Max(a, b));
        }

        public static float Max(this IEnumerable<float> source)
        {
            Check.Source(source);

            return Iterate(source, float.MinValue, (a, b) => Math.Max(a, b));
        }

        public static decimal Max(this IEnumerable<decimal> source)
        {
            Check.Source(source);

            return Iterate(source, decimal.MinValue, (a, b) => Math.Max(a, b));
        }

        public static int? Max(this IEnumerable<int?> source)
        {
            Check.Source(source);

            return IterateNullable(source, (a, b) => Math.Max(a, b));
        }

        public static long? Max(this IEnumerable<long?> source)
        {
            Check.Source(source);

            return IterateNullable(source, (a, b) => Math.Max(a, b));
        }

        public static double? Max(this IEnumerable<double?> source)
        {
            Check.Source(source);

            return IterateNullable(source, (a, b) => Math.Max(a, b));
        }

        public static float? Max(this IEnumerable<float?> source)
        {
            Check.Source(source);

            return IterateNullable(source, (a, b) => Math.Max(a, b));
        }

        public static decimal? Max(this IEnumerable<decimal?> source)
        {
            Check.Source(source);

            return IterateNullable(source, (a, b) => Math.Max(a, b));
        }

        static T? IterateNullable<T>(IEnumerable<T?> source, Func<T, T, T> selector) where T : struct
        {
            bool empty = true;
            T? value = null;
            foreach (var element in source)
            {
                if (!element.HasValue)
                    continue;

                if (!value.HasValue)
                    value = element.Value;
                else
                    value = selector(element.Value, value.Value);

                empty = false;
            }

            if (empty)
                return null;

            return value;
        }

        static TRet? IterateNullable<TSource, TRet>(
            IEnumerable<TSource> source,
            Func<TSource, TRet?> source_selector,
            Func<TRet?, TRet?, bool> selector) where TRet : struct
        {
            bool empty = true;
            TRet? value = null;
            foreach (var element in source)
            {
                TRet? item = source_selector(element);

                if (!value.HasValue)
                    value = item;
                else if (selector(item, value))
                    value = item;

                empty = false;
            }

            if (empty)
                return null;

            return value;
        }

        public static TSource Max<TSource>(this IEnumerable<TSource> source)
        {
            Check.Source(source);

            bool notAssigned = true;
            TSource maximum = default(TSource);
            int counter = 0;
            foreach (TSource element in source)
            {
                if (notAssigned)
                {
                    maximum = element;
                    notAssigned = false;
                }
                else
                {
                    int comparison;
                    if (element is IComparable<TSource>)
                        comparison = ((IComparable<TSource>)element).CompareTo(maximum);
                    else if (element is System.IComparable)
                        comparison = ((System.IComparable)element).CompareTo(maximum);
                    else
                        throw new ArgumentNullException();

                    if (comparison > 0)
                        maximum = element;
                }
                counter++;
            }

            if (counter == 0)
                throw new InvalidOperationException();
            else
                return maximum;
        }

        public static int Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            Check.SourceAndSelector(source, selector);

            return Iterate(source, int.MinValue, (a, b) => Math.Max(selector(a), b));
        }

        public static long Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
        {
            Check.SourceAndSelector(source, selector);

            return Iterate(source, long.MinValue, (a, b) => Math.Max(selector(a), b));
        }

        public static double Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            Check.SourceAndSelector(source, selector);

            return Iterate(source, double.MinValue, (a, b) => Math.Max(selector(a), b));
        }

        public static float Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
        {
            Check.SourceAndSelector(source, selector);

            return Iterate(source, float.MinValue, (a, b) => Math.Max(selector(a), b));
        }

        public static decimal Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
        {
            Check.SourceAndSelector(source, selector);

            return Iterate(source, decimal.MinValue, (a, b) => Math.Max(selector(a), b));
        }

        static U Iterate<T, U>(IEnumerable<T> source, U initValue, Func<T, U, U> selector)
        {
            bool empty = true;
            foreach (var element in source)
            {
                initValue = selector(element, initValue);
                empty = false;
            }

            if (empty)
                throw new InvalidOperationException();

            return initValue;
        }

        public static int? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
        {
            Check.SourceAndSelector(source, selector);

            return IterateNullable(source, selector, (a, b) => a > b);
        }

        public static long? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
        {
            Check.SourceAndSelector(source, selector);

            return IterateNullable(source, selector, (a, b) => a > b);
        }

        public static double? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
        {
            Check.SourceAndSelector(source, selector);

            return IterateNullable(source, selector, (a, b) => a > b);
        }

        public static float? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
        {
            Check.SourceAndSelector(source, selector);

            return IterateNullable(source, selector, (a, b) => a > b);
        }

        public static decimal? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
        {
            Check.SourceAndSelector(source, selector);

            return IterateNullable(source, selector, (a, b) => a > b);
        }

        public static TResult Max<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            Check.SourceAndSelector(source, selector);

            bool notAssigned = true;
            TResult maximum = default(TResult);
            int counter = 0;
            foreach (TSource item in source)
            {
                TResult element = selector(item);
                if (notAssigned)
                {
                    maximum = element;
                    notAssigned = false;
                }
                else
                {
                    int comparison;
                    if (element is IComparable<TResult>)
                        comparison = ((IComparable<TResult>)element).CompareTo(maximum);
                    else if (element is System.IComparable)
                        comparison = ((System.IComparable)element).CompareTo(maximum);
                    else
                        throw new ArgumentNullException();

                    if (comparison > 0)
                        maximum = element;
                }
                counter++;
            }

            if (counter == 0)
                throw new InvalidOperationException();
            else
                return maximum;
        }

        #endregion

        #region Min

        public static int Min(this IEnumerable<int> source)
        {
            Check.Source(source);

            return Iterate(source, int.MaxValue, (a, b) => Math.Min(a, b));
        }

        public static long Min(this IEnumerable<long> source)
        {
            Check.Source(source);

            return Iterate(source, long.MaxValue, (a, b) => Math.Min(a, b));
        }

        public static double Min(this IEnumerable<double> source)
        {
            Check.Source(source);

            return Iterate(source, double.MaxValue, (a, b) => Math.Min(a, b));
        }

        public static float Min(this IEnumerable<float> source)
        {
            Check.Source(source);

            return Iterate(source, float.MaxValue, (a, b) => Math.Min(a, b));
        }

        public static decimal Min(this IEnumerable<decimal> source)
        {
            Check.Source(source);

            return Iterate(source, decimal.MaxValue, (a, b) => Math.Min(a, b));
        }

        public static int? Min(this IEnumerable<int?> source)
        {
            Check.Source(source);

            return IterateNullable(source, (a, b) => Math.Min(a, b));
        }

        public static long? Min(this IEnumerable<long?> source)
        {
            Check.Source(source);

            return IterateNullable(source, (a, b) => Math.Min(a, b));
        }

        public static double? Min(this IEnumerable<double?> source)
        {
            Check.Source(source);

            return IterateNullable(source, (a, b) => Math.Min(a, b));
        }

        public static float? Min(this IEnumerable<float?> source)
        {
            Check.Source(source);

            return IterateNullable(source, (a, b) => Math.Min(a, b));
        }

        public static decimal? Min(this IEnumerable<decimal?> source)
        {
            Check.Source(source);

            return IterateNullable(source, (a, b) => Math.Min(a, b));
        }

        public static TSource Min<TSource>(this IEnumerable<TSource> source)
        {
            Check.Source(source);

            bool notAssigned = true;
            TSource minimum = default(TSource);
            int counter = 0;
            foreach (TSource element in source)
            {
                if (notAssigned)
                {
                    minimum = element;
                    notAssigned = false;
                }
                else
                {
                    int comparison;
                    if (element is IComparable<TSource>)
                        comparison = ((IComparable<TSource>)element).CompareTo(minimum);
                    else if (element is System.IComparable)
                        comparison = ((System.IComparable)element).CompareTo(minimum);
                    else
                        throw new ArgumentNullException();

                    if (comparison < 0)
                        minimum = element;
                }
                counter++;
            }

            if (counter == 0)
                throw new InvalidOperationException();
            else
                return minimum;
        }

        public static int Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            Check.SourceAndSelector(source, selector);

            return Iterate(source, int.MaxValue, (a, b) => Math.Min(selector(a), b));
        }

        public static long Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
        {
            Check.SourceAndSelector(source, selector);

            return Iterate(source, long.MaxValue, (a, b) => Math.Min(selector(a), b));
        }

        public static double Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            Check.SourceAndSelector(source, selector);

            return Iterate(source, double.MaxValue, (a, b) => Math.Min(selector(a), b));
        }

        public static float Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
        {
            Check.SourceAndSelector(source, selector);

            return Iterate(source, float.MaxValue, (a, b) => Math.Min(selector(a), b));
        }

        public static decimal Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
        {
            Check.SourceAndSelector(source, selector);

            return Iterate(source, decimal.MaxValue, (a, b) => Math.Min(selector(a), b));
        }

        public static int? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
        {
            Check.SourceAndSelector(source, selector);

            return IterateNullable(source, selector, (a, b) => a < b);
        }

        public static long? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
        {
            Check.SourceAndSelector(source, selector);

            return IterateNullable(source, selector, (a, b) => a < b);
        }

        public static float? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
        {
            Check.SourceAndSelector(source, selector);

            return IterateNullable(source, selector, (a, b) => a < b);
        }

        public static double? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
        {
            Check.SourceAndSelector(source, selector);

            return IterateNullable(source, selector, (a, b) => a < b);
        }

        public static decimal? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
        {
            Check.SourceAndSelector(source, selector);

            return IterateNullable(source, selector, (a, b) => a < b);
        }

        public static TResult Min<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            Check.SourceAndSelector(source, selector);

            bool notAssigned = true;
            TResult minimum = default(TResult);
            int counter = 0;
            foreach (TSource item in source)
            {
                TResult element = selector(item);
                if (notAssigned)
                {
                    minimum = element;
                    notAssigned = false;
                }
                else
                {
                    int comparison;
                    if (element is IComparable<TResult>)
                        comparison = ((IComparable<TResult>)element).CompareTo(minimum);
                    else if (element is System.IComparable)
                        comparison = ((System.IComparable)element).CompareTo(minimum);
                    else
                        throw new ArgumentNullException();

                    if (comparison < 0)
                        minimum = element;
                }
                counter++;
            }

            if (counter == 0)
                throw new InvalidOperationException();
            else
                return minimum;
        }

        #endregion

        #region OfType

        public static IEnumerable<TResult> OfType<TResult>(this IEnumerable source)
        {
            Check.Source(source);

            return CreateOfTypeIterator<TResult>(source);
        }

        static IEnumerable<TResult> CreateOfTypeIterator<TResult>(IEnumerable source)
        {
            foreach (object element in source)
                if (element is TResult)
                    yield return (TResult)element;
        }

        #endregion

        #region OrderBy

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source,
                Func<TSource, TKey> keySelector)
        {
            return OrderBy<TSource, TKey>(source, keySelector, null);
        }

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source,
                Func<TSource, TKey> keySelector,
                IComparer<TKey> comparer)
        {
            Check.SourceAndKeySelector(source, keySelector);

            return new OrderedSequence<TSource, TKey>(source, keySelector, comparer, SortDirection.Ascending);
        }

        #endregion

        #region OrderByDescending

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source,
                Func<TSource, TKey> keySelector)
        {
            return OrderByDescending<TSource, TKey>(source, keySelector, null);
        }

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source,
                Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            Check.SourceAndKeySelector(source, keySelector);

            return new OrderedSequence<TSource, TKey>(source, keySelector, comparer, SortDirection.Descending);
        }

        #endregion

        #region Range

        public static IEnumerable<int> Range(int start, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            long upto = ((long)start + count) - 1;

            if (upto > int.MaxValue)
                throw new ArgumentOutOfRangeException();

            return CreateRangeIterator(start, (int)upto);
        }

        static IEnumerable<int> CreateRangeIterator(int start, int upto)
        {
            for (int i = start; i <= upto; i++)
                yield return i;
        }

        #endregion

        #region Repeat

        public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException();

            return CreateRepeatIterator(element, count);
        }

        static IEnumerable<TResult> CreateRepeatIterator<TResult>(TResult element, int count)
        {
            for (int i = 0; i < count; i++)
                yield return element;
        }

        #endregion

        #region Reverse

        public static IEnumerable<TSource> Reverse<TSource>(this IEnumerable<TSource> source)
        {
            Check.Source(source);

            var list = source as IList<TSource>;
            if (list == null)
                list = new List<TSource>(source);

            return CreateReverseIterator(list);
        }

        static IEnumerable<TSource> CreateReverseIterator<TSource>(IList<TSource> source)
        {
            for (int i = source.Count; i > 0; --i)
                yield return source[i - 1];
        }

        #endregion

        #region Select

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            Check.SourceAndSelector(source, selector);

            return CreateSelectIterator(source, selector);
        }

        static IEnumerable<TResult> CreateSelectIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            foreach (var element in source)
                yield return selector(element);
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
        {
            Check.SourceAndSelector(source, selector);

            return CreateSelectIterator(source, selector);
        }

        static IEnumerable<TResult> CreateSelectIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
        {
            int counter = 0;
            foreach (TSource element in source)
            {
                yield return selector(element, counter);
                counter++;
            }
        }

        #endregion

        #region SelectMany

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            Check.SourceAndSelector(source, selector);

            return CreateSelectManyIterator(source, selector);
        }

        static IEnumerable<TResult> CreateSelectManyIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            foreach (TSource element in source)
                foreach (TResult item in selector(element))
                    yield return item;
        }

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
        {
            Check.SourceAndSelector(source, selector);

            return CreateSelectManyIterator(source, selector);
        }

        static IEnumerable<TResult> CreateSelectManyIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
        {
            int counter = 0;
            foreach (TSource element in source)
            {
                foreach (TResult item in selector(element, counter))
                    yield return item;
                counter++;
            }
        }

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> selector)
        {
            Check.SourceAndCollectionSelectors(source, collectionSelector, selector);

            return CreateSelectManyIterator(source, collectionSelector, selector);
        }

        static IEnumerable<TResult> CreateSelectManyIterator<TSource, TCollection, TResult>(IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> selector)
        {
            foreach (TSource element in source)
                foreach (TCollection collection in collectionSelector(element))
                    yield return selector(element, collection);
        }

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source,
            Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> selector)
        {
            Check.SourceAndCollectionSelectors(source, collectionSelector, selector);

            return CreateSelectManyIterator(source, collectionSelector, selector);
        }

        static IEnumerable<TResult> CreateSelectManyIterator<TSource, TCollection, TResult>(IEnumerable<TSource> source,
            Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> selector)
        {
            int counter = 0;
            foreach (TSource element in source)
                foreach (TCollection collection in collectionSelector(element, counter++))
                    yield return selector(element, collection);
        }

        #endregion

        #region Single

        static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, Fallback fallback)
        {
            var found = false;
            var item = default(TSource);

            foreach (var element in source)
            {
                if (!predicate(element))
                    continue;

                if (found)
                    throw new InvalidOperationException();

                found = true;
                item = element;
            }

            if (!found && fallback == Fallback.Throw)
                throw new InvalidOperationException();

            return item;
        }

        public static TSource Single<TSource>(this IEnumerable<TSource> source)
        {
            Check.Source(source);

            return source.Single(PredicateOf<TSource>.Always, Fallback.Throw);
        }

        public static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Check.SourceAndPredicate(source, predicate);

            return source.Single(predicate, Fallback.Throw);
        }

        #endregion

        #region SingleOrDefault

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            Check.Source(source);

            return source.Single(PredicateOf<TSource>.Always, Fallback.Default);
        }

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Check.SourceAndPredicate(source, predicate);

            return source.Single(predicate, Fallback.Default);
        }

        #endregion

        #region Skip

        public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count)
        {
            Check.Source(source);

            return CreateSkipIterator(source, count);
        }

        static IEnumerable<TSource> CreateSkipIterator<TSource>(IEnumerable<TSource> source, int count)
        {
            int i = 0;
            foreach (var element in source)
            {
                if (i++ < count)
                    continue;

                yield return element;
            }
        }

        #endregion

        #region SkipWhile

        public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Check.SourceAndPredicate(source, predicate);

            return CreateSkipWhileIterator(source, predicate);
        }

        static IEnumerable<TSource> CreateSkipWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            bool yield = false;

            foreach (TSource element in source)
            {
                if (yield)
                    yield return element;
                else
                    if (!predicate(element))
                    {
                        yield return element;
                        yield = true;
                    }
            }
        }

        public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            Check.SourceAndPredicate(source, predicate);

            return CreateSkipWhileIterator(source, predicate);
        }

        static IEnumerable<TSource> CreateSkipWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            int counter = 0;
            bool yield = false;

            foreach (TSource element in source)
            {
                if (yield)
                    yield return element;
                else
                    if (!predicate(element, counter))
                    {
                        yield return element;
                        yield = true;
                    }
                counter++;
            }
        }

        #endregion

        #region Sum

        public static int Sum(this IEnumerable<int> source)
        {
            Check.Source(source);

            return Sum<int, int>(source, (a, b) => checked(a + b));
        }

        public static int? Sum(this IEnumerable<int?> source)
        {
            Check.Source(source);

            return source.SumNullable<int?, int?>(0, (total, element) => element.HasValue ? checked(total + element) : total);
        }

        public static int Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            Check.SourceAndSelector(source, selector);

            return Sum<TSource, int>(source, (a, b) => checked(a + selector(b)));
        }

        public static int? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
        {
            Check.SourceAndSelector(source, selector);

            return source.SumNullable<TSource, int?>(0, (a, b) =>
            {
                var value = selector(b);
                return value.HasValue ? checked(a + value.Value) : a;
            });
        }

        public static long Sum(this IEnumerable<long> source)
        {
            Check.Source(source);

            return Sum<long, long>(source, (a, b) => checked(a + b));
        }

        public static long? Sum(this IEnumerable<long?> source)
        {
            Check.Source(source);

            return source.SumNullable<long?, long?>(0, (total, element) => element.HasValue ? checked(total + element) : total);
        }

        public static long Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
        {
            Check.SourceAndSelector(source, selector);

            return Sum<TSource, long>(source, (a, b) => checked(a + selector(b)));
        }

        public static long? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
        {
            Check.SourceAndSelector(source, selector);

            return source.SumNullable<TSource, long?>(0, (a, b) =>
            {
                var value = selector(b);
                return value.HasValue ? checked(a + value.Value) : a;
            });
        }

        public static double Sum(this IEnumerable<double> source)
        {
            Check.Source(source);

            return Sum<double, double>(source, (a, b) => checked(a + b));
        }

        public static double? Sum(this IEnumerable<double?> source)
        {
            Check.Source(source);

            return source.SumNullable<double?, double?>(0, (total, element) => element.HasValue ? checked(total + element) : total);
        }

        public static double Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            Check.SourceAndSelector(source, selector);

            return Sum<TSource, double>(source, (a, b) => checked(a + selector(b)));
        }

        public static double? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
        {
            Check.SourceAndSelector(source, selector);

            return source.SumNullable<TSource, double?>(0, (a, b) =>
            {
                var value = selector(b);
                return value.HasValue ? checked(a + value.Value) : a;
            });
        }

        public static float Sum(this IEnumerable<float> source)
        {
            Check.Source(source);

            return Sum<float, float>(source, (a, b) => checked(a + b));
        }

        public static float? Sum(this IEnumerable<float?> source)
        {
            Check.Source(source);

            return source.SumNullable<float?, float?>(0, (total, element) => element.HasValue ? checked(total + element) : total);
        }

        public static float Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
        {
            Check.SourceAndSelector(source, selector);

            return Sum<TSource, float>(source, (a, b) => checked(a + selector(b)));
        }

        public static float? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
        {
            Check.SourceAndSelector(source, selector);

            return source.SumNullable<TSource, float?>(0, (a, b) =>
            {
                var value = selector(b);
                return value.HasValue ? checked(a + value.Value) : a;
            });
        }

        public static decimal Sum(this IEnumerable<decimal> source)
        {
            Check.Source(source);

            return Sum<decimal, decimal>(source, (a, b) => checked(a + b));
        }

        public static decimal? Sum(this IEnumerable<decimal?> source)
        {
            Check.Source(source);

            return source.SumNullable<decimal?, decimal?>(0, (total, element) => element.HasValue ? checked(total + element) : total);
        }

        public static decimal Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
        {
            Check.SourceAndSelector(source, selector);

            return Sum<TSource, decimal>(source, (a, b) => checked(a + selector(b)));
        }

        public static decimal? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
        {
            Check.SourceAndSelector(source, selector);

            return source.SumNullable<TSource, decimal?>(0, (a, b) =>
            {
                var value = selector(b);
                return value.HasValue ? checked(a + value.Value) : a;
            });
        }

        static TR Sum<TA, TR>(this IEnumerable<TA> source, Func<TR, TA, TR> selector)
        {
            TR total = default(TR);
            long counter = 0;
            foreach (var element in source)
            {
                total = selector(total, element);
                ++counter;
            }

            return total;
        }

        static TR SumNullable<TA, TR>(this IEnumerable<TA> source, TR zero, Func<TR, TA, TR> selector)
        {
            TR total = zero;
            foreach (var element in source)
            {
                total = selector(total, element);
            }

            return total;
        }

        #endregion

        #region Take

        public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count)
        {
            Check.Source(source);

            return CreateTakeIterator(source, count);
        }

        static IEnumerable<TSource> CreateTakeIterator<TSource>(IEnumerable<TSource> source, int count)
        {
            if (count <= 0)
                yield break;

            int counter = 0;
            foreach (TSource element in source)
            {
                yield return element;

                if (++counter == count)
                    yield break;
            }
        }

        #endregion

        #region TakeWhile

        public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Check.SourceAndPredicate(source, predicate);

            return CreateTakeWhileIterator(source, predicate);
        }

        static IEnumerable<TSource> CreateTakeWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (var element in source)
            {
                if (!predicate(element))
                    yield break;

                yield return element;
            }
        }

        public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            Check.SourceAndPredicate(source, predicate);

            return CreateTakeWhileIterator(source, predicate);
        }

        static IEnumerable<TSource> CreateTakeWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            int counter = 0;
            foreach (var element in source)
            {
                if (!predicate(element, counter))
                    yield break;

                yield return element;
                counter++;
            }
        }

        #endregion

        #region ThenBy

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return ThenBy<TSource, TKey>(source, keySelector, null);
        }

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            Check.SourceAndKeySelector(source, keySelector);

            return source.CreateOrderedEnumerable(keySelector, comparer, false);
        }

        #endregion

        #region ThenByDescending

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            return ThenByDescending<TSource, TKey>(source, keySelector, null);
        }

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            Check.SourceAndKeySelector(source, keySelector);

            return source.CreateOrderedEnumerable(keySelector, comparer, true);
        }

        #endregion

        #region ToArray

        public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)
        {
            Check.Source(source);

            var collection = source as ICollection<TSource>;
            if (collection != null)
            {
                var array = new TSource[collection.Count];
                collection.CopyTo(array, 0);
                return array;
            }

            return new List<TSource>(source).ToArray();
        }

        #endregion

        #region ToDictionary
        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source,
                Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return ToDictionary<TSource, TKey, TElement>(source, keySelector, elementSelector, null);
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source,
                Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            Check.SourceAndKeyElementSelectors(source, keySelector, elementSelector);

            if (comparer == null)
                comparer = EqualityComparer<TKey>.Default;

            var dict = new Dictionary<TKey, TElement>(comparer);
            foreach (var e in source)
                dict.Add(keySelector(e), elementSelector(e));

            return dict;
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source,
                Func<TSource, TKey> keySelector)
        {
            return ToDictionary(source, keySelector, null);
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source,
                Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            return ToDictionary<TSource, TKey, TSource>(source, keySelector, Function<TSource>.Identity, comparer);
        }

        #endregion

        #region ToList
        public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source)
        {
            Check.Source(source);

            return new List<TSource>(source);
        }
        #endregion

        #region ToLookup

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return ToLookup<TSource, TKey, TSource>(source, keySelector, Function<TSource>.Identity, null);
        }

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            return ToLookup<TSource, TKey, TSource>(source, keySelector, element => element, comparer);
        }

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return ToLookup<TSource, TKey, TElement>(source, keySelector, elementSelector, null);
        }

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            Check.SourceAndKeyElementSelectors(source, keySelector, elementSelector);

            var dictionary = new Dictionary<TKey, List<TElement>>(comparer ?? EqualityComparer<TKey>.Default);
            foreach (var element in source)
            {
                var key = keySelector(element);
                if (key == null)
                    throw new ArgumentNullException("key");

                List<TElement> list;
                if (!dictionary.TryGetValue(key, out list))
                {
                    list = new List<TElement>();
                    dictionary.Add(key, list);
                }

                list.Add(elementSelector(element));
            }

            return new Lookup<TKey, TElement>(dictionary);
        }

        #endregion

        #region SequenceEqual

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            return first.SequenceEqual(second, null);
        }

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            Check.FirstAndSecond(first, second);

            if (comparer == null)
                comparer = EqualityComparer<TSource>.Default;

            using (IEnumerator<TSource> first_enumerator = first.GetEnumerator(),
                second_enumerator = second.GetEnumerator())
            {

                while (first_enumerator.MoveNext())
                {
                    if (!second_enumerator.MoveNext())
                        return false;

                    if (!comparer.Equals(first_enumerator.Current, second_enumerator.Current))
                        return false;
                }

                return !second_enumerator.MoveNext();
            }
        }

        #endregion

        #region Union

        public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            Check.FirstAndSecond(first, second);

            return first.Union(second, null);
        }

        public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            Check.FirstAndSecond(first, second);

            if (comparer == null)
                comparer = EqualityComparer<TSource>.Default;

            return CreateUnionIterator(first, second, comparer);
        }

        static IEnumerable<TSource> CreateUnionIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            var items = new HashSet<TSource>(comparer);
            foreach (var element in first)
            {
                if (!items.Contains(element))
                {
                    items.Add(element);
                    yield return element;
                }
            }

            foreach (var element in second)
            {
                if (!items.Contains(element, comparer))
                {
                    items.Add(element);
                    yield return element;
                }
            }
        }

        #endregion

        #region Where

        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Check.SourceAndPredicate(source, predicate);

            return CreateWhereIterator(source, predicate);
        }

        static IEnumerable<TSource> CreateWhereIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (TSource element in source)
                if (predicate(element))
                    yield return element;
        }

        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            Check.SourceAndPredicate(source, predicate);

            return CreateWhereIterator(source, predicate);
        }

        static IEnumerable<TSource> CreateWhereIterator<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            int counter = 0;
            foreach (TSource element in source)
            {
                if (predicate(element, counter))
                    yield return element;
                counter++;
            }
        }

        #endregion

        class ReadOnlyCollectionOf<T>
        {
            public static readonly ReadOnlyCollection<T> Empty = new ReadOnlyCollection<T>(new T[0]);
        }

        internal static ReadOnlyCollection<TSource> ToReadOnlyCollection<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                return ReadOnlyCollectionOf<TSource>.Empty;

            var ro = source as ReadOnlyCollection<TSource>;
            if (ro != null)
                return ro;

            return new ReadOnlyCollection<TSource>(source.ToArray<TSource>());
        }
    }
}

#endif
#region Full license
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#endregion
