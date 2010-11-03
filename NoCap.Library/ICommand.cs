using System;
using System.Collections.Generic;
using NoCap.Library.Util;

namespace NoCap.Library {
    /// <summary>
    /// Represents a possible processor of some data.
    /// </summary>
    /// <remarks>
    /// An <see cref="ICommand"/> may also return some data, causing it to
    /// act like a data filter.  For example, after writing the data to a file,
    /// the <see cref="ICommand"/> could return a
    /// <see cref="TypedData"/> of type <see cref="TypedDataType.Uri"/>
    /// referring to the name of the file which was written.
    /// </remarks>
    public interface ICommand : INamedComponent {
        /// <summary>
        /// Performs an operation, optionally accepting some data, and
        /// optionally returning new data.
        /// </summary>
        /// <remarks>
        /// The <see cref="Process"/> method is synchronous.  There is currently
        /// no interface to implement an asynchronous operation.  For processors
        /// which must return a value, implementors must block the operation
        /// and return the value.  If a processor does not need to return data,
        /// the operation may be asynchronous internally and must return null.
        /// </remarks>
        /// <param name="data">
        /// The data to process, if any.  Must be null or
        /// have a type in <see cref="GetInputDataTypes"/>.
        /// </param>
        /// <param name="progress">The progress tracker to be updated while the data is processed.</param>
        /// <returns>
        /// Typed data with a type in <see cref="GetOutputDataTypes"/>, or
        /// null if no data could be returned.
        /// </returns>
        TypedData Process(TypedData data, IMutableProgressTracker progress);

        /// <summary>
        /// Gets the data types which can be input into <see cref="Process"/>.
        /// </summary>
        /// <returns>
        /// A list of data types which <see cref="Process"/> accepts as its input.
        /// If <see cref="Process"/> can accept a null input,
        /// <see cref="TypedDataType.None" /> must be specified.
        /// </returns>
        IEnumerable<TypedDataType> GetInputDataTypes();

        /// <summary>
        /// Gets the possible output data types which can be given from
        /// <see cref="Process"/> given the appropriate input data type.
        /// </summary>
        /// <param name="input">The data type which would be passed into <see cref="Process"/>.</param>
        /// <returns>
        /// A list of data types which can be legally returned by <see cref="Process"/>.
        /// </returns>
        IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input);

        /// <summary>
        /// Gets a factory which can accept this instance as a parameter to the
        /// <see cref="ICommandFactory.GetCommandEditor"/> method.
        /// </summary>
        /// <remarks>
        /// The only method which is called on the value returned by this method
        /// is <see cref="ICommandFactory.GetCommandEditor"/>.
        /// </remarks>
        /// <returns>A factory instance which can provide an editor to this instance.</returns>
        ICommandFactory GetFactory();

        ITimeEstimate ProcessTimeEstimate { get; }
    }

    public static class CommandExtensions {
        public static bool HasFeatures(this ICommand command, CommandFeatures features) {
            if (command == null) {
                throw new ArgumentNullException("command");
            }

            return command.GetFactory().HasFeatures(features);
        }
    }
}