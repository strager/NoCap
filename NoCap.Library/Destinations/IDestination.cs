using System.Collections.Generic;

namespace NoCap.Library.Destinations {
    public interface IDestination {
        IOperation<TypedData> Put(TypedData data);

        IEnumerable<TypedDataType> GetInputDataTypes();
        IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input);
    }
}