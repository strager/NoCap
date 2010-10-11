namespace NoCap.Library.Sources {
    public interface ISource {
        IOperation<TypedData> Get();
    }
}
