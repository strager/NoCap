using System;
using System.Linq;

namespace NoCap.Library {
    /// <summary>
    /// Provides a set of static helper methods for <see cref="ICommand"/> instances.
    /// </summary>
    public static class Command {
        /// <summary>
        /// Gets the effective data type of the data given data.
        /// </summary>
        /// <param name="data">The data of which to get the type.</param>
        /// <returns>The type describing <see cref="data"/>.</returns>
        public static TypedDataType GetEffectiveDataType(TypedData data) {
            // TODO Move to TypedData
            return data == null ? TypedDataType.None : data.DataType;
        }

        /// <summary>
        /// Determines whether the given data is valid as input to the specified command.
        /// </summary>
        /// <remarks>
        /// This method only checks that the types are legal, not that the
        /// actual data is legal.  For example, any <see cref="TypedDataType.RawData"/>
        /// data may be given to an image uploader, but although the data may not
        /// represent image data, it will be said to be valid  by this method.
        /// </remarks>
        /// <param name="command">The command on which which the data should be tested.</param>
        /// <param name="data">The data against which the command is tested.</param>
        /// <returns>
        /// <c>true</c> if the given data is valid as input to the specified command; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidInputType(this ICommand command, TypedData data) {
            return command.IsValidInputType(GetEffectiveDataType(data));
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

        /// <summary>
        /// Determines whether the given output type is a possible output from
        /// the specified command if it is given the given input type.
        /// <!-- What a mouth-full. -->
        /// </summary>
        /// <remarks>
        /// This method only checks that the types are possible, not that the
        /// actual output will be of the expected type.
        /// </remarks>
        /// <param name="command">The command on which which the data types should be tested.</param>
        /// <param name="inputDataType">The type of the input data.</param>
        /// <param name="outputDataType">The expected type of the output data.</param>
        /// <returns>
        /// <c>true</c> if Determines whether the given output type is a possible output from
        /// the specified command if it is given the given input type; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidInputOutputType(this ICommand command, TypedDataType inputDataType, TypedDataType outputDataType) {
            if (!command.GetInputDataTypes().Contains(inputDataType)) {
                return false;
            }

            if (!command.GetOutputDataTypes(inputDataType).Contains(outputDataType)) {
                return false;
            }

            return true;
        }
    }
}