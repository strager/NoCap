using System.Collections.Generic;

namespace NoCap.Library.Sources {
    public interface ISource {
        TypedData Get(IProgressTracker progress);

        IEnumerable<TypedDataType> GetOutputDataTypes();
    }
}
