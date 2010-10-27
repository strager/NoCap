using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NoCap.Library {
    public interface IInfoStuff {
        ObservableCollection<ICommand> Commands { get; }
        IEnumerable<ICommandFactory> CommandFactories { get; }
    }
}