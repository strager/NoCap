using System.Collections.Generic;
using NoCap.Library.Util;

namespace NoCap.Library {
    /// <summary>
    /// Represents a possible destination of some data.
    /// </summary>
    /// <remarks>
    /// An <see cref="IDestination"/> may also return some data, causing it to
    /// act like a data filter.  For example, after writing the data to a file,
    /// the <see cref="IDestination"/> could return a
    /// <see cref="TypedData"/> of type <see cref="TypedDataType.Uri"/>
    /// referring to the name of the file which was written.
    /// </remarks>
    public interface IDestination {
        // TODO INamedComponent

        /// <summary>
        /// Stores the specified data somewhere, optionally returning new data.
        /// </summary>
        /// <remarks>
        /// The <see cref="Put"/> method is synchronous.  There is currently no
        /// interface to implement an asynchronous operation.  For destinations
        /// which must return a value, implementors must block the operation
        /// and return the value.  If a destination does not need to return data,
        /// the operation may be asynchronous internally and must return null.
        /// </remarks>
        /// <param name="data">The data to store. Must have a type in <see cref="GetInputDataTypes"/>.</param>
        /// <param name="progress">The progress tracker to be updated while the data is stored.</param>
        /// <returns>
        /// Typed data with a type in <see cref="GetOutputDataTypes"/>, or
        /// null if no data could be returned.
        /// </returns>
        TypedData Put(TypedData data, IMutableProgressTracker progress);

        /// <summary>
        /// Gets the data types which can be input into <see cref="Put"/>.
        /// </summary>
        /// <returns>
        /// A list of data types which <see cref="Put"/> accepts as its input.
        /// </returns>
        IEnumerable<TypedDataType> GetInputDataTypes();

        /// <summary>
        /// Gets the possible output data types which can be given from
        /// <see cref="Put"/> given the appropriate input data type.
        /// </summary>
        /// <param name="input">The data type which would be passed into <see cref="Put"/>.</param>
        /// <returns>
        /// A list of data types which can be legally returned by <see cref="Put"/>.
        /// </returns>
        IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input);
    }

    public static class DestinationHelpers {
        public static TypedData RouteFrom(this IDestination destination, ISource source, IMutableProgressTracker progress) {
            var sourceProgress = new NotifyingProgressTracker();
            var destProgress = new NotifyingProgressTracker();

            var aggregateProgress = new AggregateProgressTracker(sourceProgress, destProgress);
            aggregateProgress.BindTo(progress);

            var data = source.Get(sourceProgress);

            if (data == null) {
                return null;
            }

            data = destination.Put(data, destProgress);

            return data;
        }
    }
}