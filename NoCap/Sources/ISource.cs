namespace NoCap.Sources {
    public interface ISource {
        IOperation<TypedData> Get();
    }
}
