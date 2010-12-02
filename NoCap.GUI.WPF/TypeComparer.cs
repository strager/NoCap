using System.Collections.Generic;

namespace NoCap.GUI.WPF {
    public class TypeComparer<T> : EqualityComparer<T>
        where T : class {
        public override bool Equals(T x, T y) {
            if (x == null && y == null) {
                return true;
            }

            if (x == null || y == null) {
                return false;
            }

            return x.GetType().Equals(y.GetType());
        }

        public override int GetHashCode(T obj) {
            return 0;
        }
    }
}