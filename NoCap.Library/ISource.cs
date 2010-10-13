using System.Collections.Generic;
using NoCap.Library.Util;

namespace NoCap.Library {
    public interface ISource {
        TypedData Get(IMutableProgressTracker progress);

        IEnumerable<TypedDataType> GetOutputDataTypes();
    }
}
