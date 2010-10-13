using System.Collections.Generic;

namespace NoCap.Library.Sources {
    public interface ISource {
        TypedData Get(IMutableProgressTracker progress);

        IEnumerable<TypedDataType> GetOutputDataTypes();
    }
}
