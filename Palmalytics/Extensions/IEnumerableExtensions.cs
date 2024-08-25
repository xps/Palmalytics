using System;
using System.Collections.Generic;
using System.Linq;

namespace Palmalytics.Extensions
{
    internal static class IEnumerableExtensions
    {
        /// <summary>
        /// Calls string.Join on the items of the enumerable.
        /// </summary>
        public static string Join<T>(this IEnumerable<T> enumerable, string separator)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
            if (separator == null) throw new ArgumentNullException(nameof(separator));

            return string.Join(separator, enumerable);
        }

        /// <summary>
        /// Calls string.Join on the items of the enumerable, first applying the selector.
        /// </summary>
        public static string Join<T>(this IEnumerable<T> enumerable, Func<T, object> selector, string separator)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
            if (separator == null) throw new ArgumentNullException(nameof(separator));

            return string.Join(separator, enumerable.Select(selector));
        }
    }
}
