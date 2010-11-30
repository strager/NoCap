namespace Bindable.Linq.Helpers
{
    using System.ComponentModel;

    internal static class PropertyChangedCache
    {
        public static readonly PropertyChangedEventArgs Count = new PropertyChangedEventArgs("Count");
        public static readonly PropertyChangedEventArgs IsLoading = new PropertyChangedEventArgs("IsLoading");
    }
}