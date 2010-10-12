using System.Collections.Generic;

namespace NoCap.Library.Destinations {
    public interface IDestination {
        TypedData Put(TypedData data, IProgressTracker progress);

        IEnumerable<TypedDataType> GetInputDataTypes();
        IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input);
    }
}