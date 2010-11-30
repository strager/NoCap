namespace Bindable.Linq.Iterators
{
    /// <summary>
    /// Represents the different options when an iterator is told to load its source collections.
    /// </summary>
    public enum LoadState
    {
        /// <summary>
        /// Indicates that only collections that haven't been loaded yet should be loaded.
        /// </summary>
        IfNotAlreadyLoaded,
        /// <summary>
        /// Indicates that all collections should be loaded.
        /// </summary>
        EvenIfLoaded
    }
}