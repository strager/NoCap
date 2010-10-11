namespace NoCap.Library.Destinations {
    public interface IDestination {
        IOperation<TypedData> Put(TypedData data);
    }
}