using System.Runtime.Serialization;

namespace NoCap.Library.Util {
    public static class SerializationExtensions {
        public static T GetValue<T>(this SerializationInfo info, string name) {
            return (T) info.GetValue(name, typeof(T));
        }
    }
}