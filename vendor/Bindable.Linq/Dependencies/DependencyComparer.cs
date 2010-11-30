namespace Bindable.Linq.Dependencies
{
    using System.Collections.Generic;

    /// <summary>
    /// Compares dependencies.
    /// </summary>
    internal sealed class DependencyComparer : IEqualityComparer<IDependencyDefinition>
    {
        public static readonly DependencyComparer _instance = new DependencyComparer();

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyComparer"/> class.
        /// </summary>
        private DependencyComparer() {}

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static DependencyComparer Instance
        {
            get { return _instance; }
        }

        #region IEqualityComparer<IDependencyDefinition> Members
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type <paramref name="T"/> to compare.</param>
        /// <param name="y">The second object of type <paramref name="T"/> to compare.</param>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        public bool Equals(IDependencyDefinition x, IDependencyDefinition y)
        {
            return (x == null && y == null) || (x != null && y != null && x.ToString() == y.ToString());
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
        public int GetHashCode(IDependencyDefinition obj)
        {
            return obj.ToString().GetHashCode();
        }
        #endregion
    }
}