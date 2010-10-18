using WinputDotNet;

namespace NoCap.GUI.WPF.Templates {
    interface ITemplateFactory : INamedComponent {
        ITemplate CreateTemplate();

        // TODO Load template ?

        // TODO Template GUI
    }
}
