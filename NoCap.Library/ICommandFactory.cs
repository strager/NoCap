﻿using System;

namespace NoCap.Library {
    /// <summary>
    /// Represents a factory which can produce instances of commands
    /// and editors of those command instances.
    /// </summary>
    public interface ICommandFactory : INamedComponent {
        /// <summary>
        /// Creates a new command instance.
        /// </summary>
        /// <returns>A new command instance.</returns>
        ICommand CreateCommand();

        void PopulateCommand(ICommand command, IInfoStuff infoStuff);

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

    public static class CommandFactoryExtensions {
        public static bool HasFeatures(this ICommandFactory factory, CommandFeatures features) {
            if (factory == null) {
                throw new ArgumentNullException("factory");
            }

            return factory.CommandFeatures.HasFlag(features);
        }

        public static ICommand CreateCommand(this ICommandFactory factory, IInfoStuff infoStuff) {
            if (factory == null) {
                throw new ArgumentNullException("factory");
            }

            var command = factory.CreateCommand();
            factory.PopulateCommand(command, infoStuff);

            return command;
        }
    }
}
