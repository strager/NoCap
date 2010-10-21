using System;
using System.Linq;

namespace NoCap.Library {
    /// <summary>
    /// Provides a set of static helper methods for <see cref="IProcessor"/> instances.
    /// </summary>
    public static class Processor {
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
        /// Determines whether the given data is valid as input to the specified processor.
        /// </summary>
        /// <remarks>
        /// This method only checks that the types are legal, not that the
        /// actual data is legal.  For example, any <see cref="TypedDataType.RawData"/>
        /// data may be given to an image uploader, but although the data may not
        /// represent image data, it will be said to be valid  by this method.
        /// </remarks>
        /// <param name="processor">The processor on which which the data should be tested.</param>
        /// <param name="data">The data against which the processor is tested.</param>
        /// <returns>
        /// <c>true</c> if the given data is valid as input to the specified processor; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidInputType(this IProcessor processor, TypedData data) {
            var type = GetEffectiveDataType(data);
            var inputTypes = processor.GetInputDataTypes();

            return inputTypes.Contains(type) || (type == TypedDataType.None && !inputTypes.Any());
        }

        /// <summary>
        /// Checks whether the given data is valid as input to the specified processor.
        /// Throws an exception if the data is invalid.
        /// </summary>
        /// <param name="processor">The processor on which which the data should be tested.</param>
        /// <param name="data">The data against which the processor is tested.</param>
        /// <seealso cref="IsValidInputType"/>
        /// <exception cref="InvalidOperationException">
        /// The given data is valid as input to the specified processor.
        /// </exception>
        public static void CheckValidInputType(this IProcessor processor, TypedData data) {
            if (!processor.IsValidInputType(data)) {
                throw new InvalidOperationException("Invalid data type");
            }
        }

        /// <summary>
        /// Determines whether the given output type is a possible output from
        /// the specified processor if it is given the given input type.
        /// <!-- What a mouth-full. -->
        /// </summary>
        /// <remarks>
        /// This method only checks that the types are possible, not that the
        /// actual output will be of the expected type.
        /// </remarks>
        /// <param name="processor">The processor on which which the data types should be tested.</param>
        /// <param name="inputDataType">The type of the input data.</param>
        /// <param name="outputDataType">The expected type of the output data.</param>
        /// <returns>
        /// <c>true</c> if Determines whether the given output type is a possible output from
        /// the specified processor if it is given the given input type; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidInputOutputType(this IProcessor processor, TypedDataType inputDataType, TypedDataType outputDataType) {
            if (!processor.GetInputDataTypes().Contains(inputDataType)) {
                return false;
            }

            if (!processor.GetOutputDataTypes(inputDataType).Contains(outputDataType)) {
                return false;
            }

            return true;
        }
    }
}