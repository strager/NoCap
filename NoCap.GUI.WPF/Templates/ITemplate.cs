using NoCap.GUI.WPF.Settings;

namespace NoCap.GUI.WPF.Templates {
    interface ITemplate {
        string Name { get; set; }

        SourceDestinationCommand GetCommand();
    }
}
