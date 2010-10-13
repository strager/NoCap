using System;
using System.Collections.Generic;

namespace NoCap.Library.Util {
    public static class LinqHelpers {
        public static IEnumerable<T> Unique<T>(this IEnumerable<T> source) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }

            var uniqueItems = new HashSet<T>(EqualityComparer<T>.Default);

            foreach (var element in source) {
                uniqueItems.Add(element);
            }

            return uniqueItems;
        }
    }
}