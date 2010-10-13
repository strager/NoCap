using System.Collections.Generic;

namespace NoCap.Library.Util {
    public static class LinqHelpers {
        public static IEnumerable<T> Unique<T>(this IEnumerable<T> source) {
            var uniqueItems = new HashSet<T>();

            foreach (T element in source) {
                uniqueItems.Add(element);
            }

            return uniqueItems;
        }
    }
}