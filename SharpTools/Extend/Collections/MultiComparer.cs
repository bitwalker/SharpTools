using System;
using System.Collections;
using System.Collections.Generic;

namespace SharpTools.Extend.Collections
{
    /// <summary>
    /// Merges two or more comparers into a single comparer, applying them in order.
    /// Comparisons are performed in the order they were provided, but subsequent
    /// comparisons are only performed if the previous one returned 0, i.e. equivalent.
    ///
    /// The purpose of this class is to allow one to perform complex comparisons beyond
    /// a simple ascending or descending ordering. A few examples of such comparisons are:
    /// 
    /// - sort a list of Orders by ascending OrderDate, but if two Orders were placed on
    ///   the same day, sort them by OrderStatus in order of priority
    /// - sort a list of People by FirstName, then by LastName, then by DateOfBirth in
    ///   reverse chronological order (youngest people appear first).
    ///
    /// As you can see, combined comparisons like those above are exceedingly common, but
    /// unless you want to hand write multiple <see cref="IComparer{T}"/> implementations,
    /// one for each combination of conditions, you are going to want something better. That's
    /// what <see cref="MultiComparer{T}"/> aims to provide!
    /// </summary>
    /// <typeparam name="T">The type to compare</typeparam>
    public sealed class MultiComparer<T> : IComparer<T>, IComparer
    {
        private readonly IComparer<T>[] _comparers; 

        /// <summary>
        /// Creates a <see cref="MultiComparer{T}"/> given a collection of <see cref="IComparer{T}"/>.
        /// </summary>
        /// <param name="comparers">The comparers to use, in order of priority</param>
        public MultiComparer(params IComparer<T>[] comparers)
        {
            if (comparers == null || comparers.Length < 2)
                throw new ArgumentException("You must provide at least two comparers!");

            _comparers = comparers;
        }

        /// <summary>
        /// Create a <see cref="MultiComparer{T}"/> from two lambda expressions.
        /// Optionally provide a flag to sort in ascending/descending order (default is ascending).
        /// This is a shortcut for creating two <see cref="GenericComparer{T}"/> and providing them
        /// as constructor parameters to <see cref="MultiComparer{T}"/>
        /// </summary>
        /// <typeparam name="U">The result type of the primary expression</typeparam>
        /// <typeparam name="V">The result type of the secondary expression</typeparam>
        /// <param name="extract1">The primary value expression</param>
        /// <param name="extract2">The secondary value expression</param>
        /// <param name="isAscendingOrder">Defines whether comparisons use default (ascending) or reverse (descending) behavior</param>
        /// <returns><see cref="MultiComparer{T}"/></returns>
        public static MultiComparer<T> Create<U, V>(
            Func<T, U> extract1, Func<T, V> extract2, bool isAscendingOrder = true
        ) where U : IComparable, IComparable<U> 
          where V : IComparable, IComparable<V>
        {
            var compare1 = GenericComparer<T>.Create(extract1);
            var compare2 = GenericComparer<T>.Create(extract2);
            return new MultiComparer<T>(compare1, compare2);
        }

        public int Compare(T x, T y)
        {
            int result = 0;
            int cursor = 0;
            do
            {
                result = _comparers[cursor].Compare(x, y);
                cursor++;
            }
            while (result == 0 && cursor < _comparers.Length);

            return result;
        }

        int IComparer.Compare(object x, object y)
        {
            if ((x is T) == false)
                throw new ArgumentException(string.Format("x is not an instance of type {0}", typeof(T).Name));
            if ((y is T) == false)
                throw new ArgumentException(string.Format("y is not an instance of type {0}", typeof(T).Name));

            return Compare((T) x, (T) y);
        }
    }
}
