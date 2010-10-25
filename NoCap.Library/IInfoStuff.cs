using System.Collections.ObjectModel;

namespace NoCap.Library {
    public interface IInfoStuff {
        ObservableCollection<ICommand> Processors { get; }
    }
}