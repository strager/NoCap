using System;
using System.Collections.Generic;
using System.Threading;
using NoCap.Library.Extensions;
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
        /// no interface to implement an asynchronous operation.  For commands
        /// which must return a value, implementors must block the operation
        /// and return the value.  If a command does not need to return data,
        /// the operation may be asynchronous internally and must return null.
        /// </remarks>
        /// <remarks>
        /// Callers of <see cref="Process"/> own the <see cref="TypedData"/> returned.
        /// They are responsible for disposing of the data by calling
        /// <see cref="IDisposable.Dispose"/> appropriately.
        /// </remarks>
        /// <param name="data">The data to process, if any.</param>
        /// <param name="progress">The progress tracker to be updated while the data is processed.</param>
        /// <param name="cancelToken"></param>
        /// <returns>
        /// Typed data representing the result of the operation, or
        /// null if no data could be returned.
        /// </returns>
        TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken);

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

        bool IsValid();
    }

    public static class CommandExtensions {
        public static bool HasFeatures(this ICommand command, CommandFeatures features) {
            if (command == null) {
                throw new ArgumentNullException("command");
            }

            return command.GetFactory().HasFeatures(features);
        }

        public static bool IsValidAndNotNull(this ICommand command) {
            return command != null && command.IsValid();
        }
    }
}