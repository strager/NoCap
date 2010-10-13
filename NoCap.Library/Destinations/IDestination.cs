using System.Collections.Generic;

namespace NoCap.Library.Destinations {
    public interface IDestination {
        TypedData Put(TypedData data, IMutableProgressTracker progress);

        IEnumerable<TypedDataType> GetInputDataTypes();
        IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input);
    }
}