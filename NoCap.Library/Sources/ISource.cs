using System.Collections.Generic;

namespace NoCap.Library.Sources {
    public interface ISource {
        IOperation<TypedData> Get();

        IEnumerable<TypedDataType> GetOutputDataTypes();
    }
}
