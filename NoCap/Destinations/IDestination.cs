namespace NoCap.Destinations {
    public interface IDestination {
        IOperation Put(TypedData data);
    }
}