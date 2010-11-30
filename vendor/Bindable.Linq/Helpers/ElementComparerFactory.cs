namespace Bindable.Linq.Helpers
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A comparer used for comparing elements within a collection for equality. This is often used for IndexOf() operations, the goal 
    /// being to find the index of a particular item, not the index of anything matching the particular item (in the case of Bindable LINQ 
    /// result sets, that is). 
    /// </summary>
    internal static class ElementComparerFactory
    {
        public static IEqualityComparer<TElement> Create<TElement>()
        {
            if (typeof (TElement).IsValueType)
            {
                return new ValueTypeComparer<TElement>();
            }
            else
            {
                return new ReferenceTypeComparer<TElement>();
            }
        }

        #region Nested type: ReferenceTypeComparer
        private class ReferenceTypeComparer<TElement> : IEqualityComparer<TElement>
        {
            #region IEqualityComparer<TElement> Members
            public bool Equals(TElement x, TElement y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(TElement obj)
            {
                return obj.GetHashCode();
            }
            #endregion
        }
        #endregion

        #region Nested type: ValueTypeComparer
        private class ValueTypeComparer<TElement> : IEqualityComparer<TElement>
        {
            #region IEqualityComparer<TElement> Members
            public bool Equals(TElement x, TElement y)
            {
                return object.Equals(x, y);
            }

            public int GetHashCode(TElement obj)
            {
                throw new NotImplementedException();
            }
            #endregion
        }
        #endregion
    }
}