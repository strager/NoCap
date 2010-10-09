namespace NoCap.Destinations {
    public interface IDestination {
        bool Put(DestinationType type, object data, string name, IResultThing result);
    }

    public enum DestinationType {
        Any = 0,
        Image,
        Text,

        // Add new destination types here

        User = 9001
    }
}