namespace NoCap.Destinations {
    public interface IDestination {
        IOperation<TypedData> Put(TypedData data);
    }
}