using System.Collections.Generic;
using NoCap.Library.Util;

namespace NoCap.Library {
    public interface IDestination {
        TypedData Put(TypedData data, IMutableProgressTracker progress);

        IEnumerable<TypedDataType> GetInputDataTypes();
        IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input);
    }
}