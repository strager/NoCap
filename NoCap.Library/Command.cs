using System;
using System.Linq;

namespace NoCap.Library {
    /// <summary>
    /// Provides a set of static helper methods for <see cref="ICommand"/> instances.
    /// </summary>
    public static class Command {
        public static Predicate<object> GetHasFeaturesPredicate(CommandFeatures features) {
            return (obj) => {
                var objAsCommand = obj as ICommand;

                if (objAsCommand != null) {
                    return objAsCommand.HasFeatures(features);
                }

                var objAsFactory = obj as ICommandFactory;

                if (objAsFactory != null) {
                    return objAsFactory.HasFeatures(features);
                }

                return false;
            };
        }

        /// <summary>
        /// Determines whether the given data is valid as input to the specified command.
        /// </summary>
        /// <remarks>
        /// This method only checks that the types are legal, not that the
        /// actual data is legal.  For example, any <see cref="TypedDataType.Stream"/>
        /// data may be given to an image converter, but although the data may not
        /// represent image data, it will be said to be valid by this method.
        /// </remarks>
        /// <param name="command">The command on which which the data should be tested.</param>
        /// <param name="data">The data against which the command is tested.</param>
        /// <returns>
        /// <c>true</c> if the given data is valid as input to the specified command; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidInputType(this ICommand command, TypedData data) {
            return command.IsValidInputType(data.GetEffectiveDataType());
        }

        public static bool IsValidInputType(this ICommand command, TypedDataType type) {
            var inputTypes = command.GetInputDataTypes();

            return inputTypes.Contains(type) || (type == TypedDataType.None && !inputTypes.Any());
        }

        /// <summary>
        /// Checks whether the given data is valid as input to the specified command.
        /// Throws an exception if the data is invalid.
        /// </summary>
        /// <param name="command">The command on which which the data should be tested.</param>
        /// <param name="data">The data against which the command is tested.</param>
        /// <seealso cref="IsValidInputType"/>
        /// <exception cref="InvalidOperationException">
        /// The given data is valid as input to the specified command.
        /// </exception>
        public static void CheckValidInputType(this ICommand command, TypedData data) {
            if (!command.IsValidInputType(data)) {
                throw new InvalidOperationException("Invalid data type");
            }
        }

        public static void CheckValidInputType(this ICommand command, TypedDataType type) {
            if (!command.IsValidInputType(type)) {
                throw new InvalidOperationException("Invalid data type");
            }
        }
    }
}