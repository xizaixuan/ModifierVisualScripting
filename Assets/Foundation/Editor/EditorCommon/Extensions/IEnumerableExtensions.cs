
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEditor.Modifier.EditorCommon.Extensions
{
    public static class IEnumerableExtensions
    {
        internal static IEnumerable<T> OfExactType<T>(this IEnumerable source)
        {
            if (source == null)
            {
                throw new ArgumentException("Must specify a valid source", nameof(source));
            }

            return OfExactTypeIterator<T>(source);
        }

        static IEnumerable<T> OfExactTypeIterator<T>(IEnumerable source)
        {
            return source.OfType<T>().Where(obj => obj.GetType() == typeof(T));
        }
    }
}