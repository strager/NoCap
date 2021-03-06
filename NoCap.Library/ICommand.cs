﻿using System;
using System.Threading;
using NoCap.Library.Extensions;
using NoCap.Library.Progress;
using NoCap.Library.Util;

namespace NoCap.Library {
    /// <summary>
    /// Represents a transformer of some data.
    /// </summary>
    /// <remarks>
    /// An <see cref="ICommand"/> may return some data, causing it to act
    /// like a data filter.  For example, after writing the data to a file,
    /// the <see cref="ICommand"/> could return a <see cref="TypedData"/>
    /// of type <see cref="TypedDataType.Uri"/> referring to the name of
    /// the file which was written.
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
        /// <param name="cancelToken">The token used to detect cancellations of the operation.</param>
        /// <returns>
        /// Typed data representing the result of the operation, or
        /// null if no data could be returned.
        /// </returns>
        TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken);

        /// <summary>
        /// Gets a command factory which corresponds to this instance.
        /// </summary>
        /// <returns>A factory instance which can provide other instances of this type.</returns>
        ICommandFactory GetFactory();

        /// <summary>
        /// Gets the estimated time a call to <see cref="Process"/> will take.
        /// </summary>
        /// <value>The time estimate of <see cref="Process"/>.</value>
        ITimeEstimate ProcessTimeEstimate { get; }

        /// <summary>
        /// Determines whether this instance is valid and <see cref="Process"/>
        /// can be safely called.
        /// </summary>
        /// <remarks>
        /// An instance is considered invalid if calling <see cref="Process"/>
        /// would always result in an error, typically due to invalid configuration.
        /// An instance is not considered invalid if the environment disallows
        /// the operation from being performed, e.g. by the network's firewall
        /// blocking access to an external server used during the call to
        /// <see cref="Process"/>.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        bool IsValid();
    }
}