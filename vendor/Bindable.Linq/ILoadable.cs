namespace Bindable.Linq
{
    /// <summary>
    /// Implemented by objects that can have a loading state.
    /// </summary>
    public interface ILoadable
    {
        /// <summary>
        /// Gets a value indicating whether this instance is loading.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is loading; otherwise, <c>false</c>.
        /// </value>
        bool IsLoading { get; }
    }
}