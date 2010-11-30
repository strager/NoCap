namespace Bindable.Linq.Helpers
{
    using System;

    /// <summary>
    /// Represents an exception thrown when a Bindable LINQ object attempts to acquire a <see cref="System.Threading.Monitor"/> lock
    /// but a timeout occurs.
    /// </summary>
    public sealed class LockAttemptTimeoutException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LockAttemptTimeoutException"/> class.
        /// </summary>
        public LockAttemptTimeoutException() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="LockAttemptTimeoutException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public LockAttemptTimeoutException(string message)
            : base(message) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="LockAttemptTimeoutException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public LockAttemptTimeoutException(string message, Exception inner)
            : base(message, inner) {}
    }
}