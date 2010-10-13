using System.Collections.Generic;
using NoCap.Library.Util;

namespace NoCap.Library {
    /// <summary>
    /// Represents a source of typed data.
    /// </summary>
    public interface ISource {
        /// <summary>
        /// Fetches some typed data from a data source.
        /// </summary>
        /// <remarks>
        /// The <see cref="Get"/> method is synchronous.  There is currently no
        /// interface to implement an asynchronous operation.  Implementors
        /// must block the operation and return the value, or null.
        /// </remarks>
        /// <param name="progress">The progress tracker to be updated while the data is fetched.</param>
        /// <returns>
        /// Typed data with a type in <see cref="GetOutputDataTypes"/>, or
        /// null if no data could be fetched.
        /// </returns>
        TypedData Get(IMutableProgressTracker progress);

        /// <summary>
        /// Gets all the possible output data types.
        /// </summary>
        /// <returns>
        /// A list of data types which can be legally returned by
        /// <see cref="Get"/>.
        /// </returns>
        IEnumerable<TypedDataType> GetOutputDataTypes();
    }
}
