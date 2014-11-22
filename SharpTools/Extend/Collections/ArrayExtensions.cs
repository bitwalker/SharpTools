using System;

namespace SharpTools.Extend.Collections
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Takes a slice of an array given a starting index and count.
        /// If the starting index is less than 0, then count elements are sliced from the source.
        /// If the starting index is greater than the upper bound of the source, an empty array is returned.
        /// If the count is less than 0, an empty array is returned
        /// If the count is greater than the upper bound of the source, then as many elements as possible are sliced from the source.
        /// In all other cases, the slice is the exact size specified by count, unless the number of elements available is less, in
        /// which case, the slice will be the size of the remaining number of available elements.
        /// 
        /// Some examples:
        /// Given 100 elements, with a starting index of 75, and a count of 30, a slice 25 elements long is returned
        /// Given 100 elements, with a staring index of 100, and a count of 10, an empty slice is returned
        /// Given 100 elements, with a starting index of -10, and a count of 10, a slice 10 elements long is returned, from the start of the source array
        /// Given 100 elements, with a starting index of 50, and a count of 100, a slice 50 elements long is returned
        /// </summary>
        /// <typeparam name="T">The type of the source and result arrays</typeparam>
        /// <param name="source">The source array</param>
        /// <param name="startIndex">The starting index of the slice into the source array</param>
        /// <param name="count">The number of elements to slice from the source array</param>
        /// <returns><see cref="T[]"/></returns>
        public static T[] Slice<T>(this T[] source, long startIndex, long count = 0)
        {
            // If the slice index is past the end of the array, return an empty array
            if (startIndex >= source.Length || count < 0)
                return new T[0];
            // If the slice index is before the start of the array, we're going to grab the whole thing
            if (startIndex < 0)
                startIndex = 0;
            // If the count is less than 0 or greater than the length of the source, we're going to
            // take everything we can past the startIndex
            if (count > source.Length)
                count = 0;

            // If the start index and count are 0 return a copy of the array
            if (startIndex == 0 && count == 0)
            {
                var copy = new T[source.Length];
                source.CopyTo(copy, 0);
                return copy;
            }

            // Create a new array with the exact amount of elements that
            // we're taking from the source array, and copy to it
            var maxAvailable = (source.Length - startIndex);
            var taking       = maxAvailable - (maxAvailable - count);
            var buffer       = new T[taking];
            var i = startIndex;
            var j = 0;
            for (; j < taking; i++, j++)
            {
                buffer[j] = source[i];
            }

            return buffer;
        }

        /// <summary>
        /// Combines a slice and map operation into one. Assumes a slice of the max number of
        /// elements available past the starting index of the slice.
        /// The slice semantics are the same as <see cref="ArrayExtensions.Slice"/>.
        /// If no transform function is provided, no transform will be applied, and it will
        /// behave just like a standard slice.
        /// </summary>
        /// <typeparam name="T">The type of the source array</typeparam>
        /// <typeparam name="U">The type of the result array</typeparam>
        /// <param name="source">The source array</param>
        /// <param name="startIndex">The starting index of the slice into the source array</param>
        /// <param name="transform">The transformation function to apply to each element. Optional.</param>
        /// <returns><see cref="U[]"/></returns>
        public static U[] SliceMap<T, U>(this T[] source, long startIndex, Func<T, U> transform)
        {
            return SliceMap<T, U>(source, startIndex, count: 0, transform: transform);
        }

        /// <summary>
        /// Combines a slice and map operation into one.
        /// The slice semantics are the same as <see cref="ArrayExtensions.Slice"/>.
        /// If no transform function is provided, no transform will be applied, and it will
        /// behave just like a standard slice.
        /// </summary>
        /// <typeparam name="T">The type of the source array</typeparam>
        /// <typeparam name="U">The type of the result array</typeparam>
        /// <param name="source">The source array</param>
        /// <param name="startIndex">The starting index of the slice into the source array</param>
        /// <param name="count">The number of elements to slice from the source array</param>
        /// <param name="transform">The transformation function to apply to each element. Optional.</param>
        /// <returns><see cref="U[]"/></returns>
        public static U[] SliceMap<T, U>(this T[] source, long startIndex, long count, Func<T, U> transform)
        {
            // If the slice index is past the end of the array, return an empty array
            if (startIndex >= source.Length || count < 0)
                return new U[0];
            // If the slice index is before the start of the array, we're going to grab the whole thing
            if (startIndex < 0)
                startIndex = 0;
            // If the count is less than 0 or greater than the length of the source, we're going to
            // take everything we can past the startIndex
            if (count > source.Length)
                count = 0;

            // If the start index and count are 0 take everything
            if (startIndex == 0 && count == 0)
                count = source.Length;

            // Create a new array with the exact amount of elements that
            // we're taking from the source array, and copy to it
            var maxAvailable = (source.Length - startIndex);
            var taking       = maxAvailable - (maxAvailable - count);
            var buffer       = new U[taking];
            var i = startIndex;
            var j = 0;
            for (; j < taking; i++, j++)
            {
                buffer[j] = transform(source[i]);
            }

            return buffer;
        }
    }
}
