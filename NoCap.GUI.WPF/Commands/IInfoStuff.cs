using System.Collections.ObjectModel;
using NoCap.Library;

namespace NoCap.GUI.WPF.Commands {
    public interface IInfoStuff {
        ObservableCollection<IProcessor> Processors { get; }
    }
}