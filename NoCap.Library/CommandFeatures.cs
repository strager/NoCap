using System;

namespace NoCap.Library {
    /// <summary>
    /// Represents a feature a command supports.
    /// </summary>
    /// <remarks>
    /// <see cref="CommandFeatures"/> are used to identify which commands support
    /// certain operations.  Using command features allows commands and plugins
    /// to collaborate without the need for implementing and casting interfaces.
    /// </remarks>
    [Flags]
    public enum CommandFeatures {
        /// <summary>The command supports no significant features.</summary>
        None = 0,

        /// <summary>The command features image uploading capabilities.</summary>
        ImageUploader = (1 << 0),

        /// <summary>The command features file uploading capabilities.</summary>
        FileUploader = (1 << 1),

        /// <summary>The command features the ability to shorten URLs.</summary>
        UrlShortener = (1 << 2),

        /// <summary>The command features text uploading capabilities.</summary>
        TextUploader = (1 << 3),

        /// <summary>The command can and should be run on its own with no inputs or outputs.</summary>
        StandAlone = (1 << 8),
    };
}