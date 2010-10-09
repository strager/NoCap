namespace NoCap {
    public interface IResultThing {
        void Start();
        void Progress(float progress);
        void Done(string reference);
        void Error(string message);
    }
}