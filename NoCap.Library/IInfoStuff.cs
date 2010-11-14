using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NoCap.Library {
    public interface IInfoStuff {
        IEnumerable<ICommandFactory> CommandFactories { get; }

        ICommand GetDefaultCommand(CommandFeatures features);
        bool IsDefaultCommand(ICommand command);
    }
}