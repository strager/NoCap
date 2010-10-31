﻿using System;

namespace NoCap.Library {
    [Flags]
    public enum CommandFeatures {
        ImageUploader = (1 << 0),
        FileUploader = (1 << 1),
        UrlShortener = (1 << 2),
        TextUploader = (1 << 3),

        StandAlone = (1 << 8),
    };

    /// <summary>
    /// Represents a factory which can produce instances of commands
    /// and editors of those command instances.
    /// </summary>
    public interface ICommandFactory : INamedComponent {
        /// <summary>
        /// Creates a new command instance.
        /// </summary>
        /// <param name="infoStuff"></param>
        /// <returns>A new command instance.</returns>
        ICommand CreateCommand(IInfoStuff infoStuff);

        /// <summary>
        /// Gets an editor which can be used to edit the given command
        /// instance, or <c>null</c> if no editor is needed or can be provided.
        /// </summary>
        /// <param name="command">The command to edit.</param>
        /// <param name="infoStuff"></param>
        /// <returns>A new instance of an editor for <paramref name="command"/>, or <c>null</c>.</returns>
        ICommandEditor GetCommandEditor(ICommand command, IInfoStuff infoStuff);

        CommandFeatures CommandFeatures { get; }
    }
}
