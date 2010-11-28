using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace NoCap.Library {
    public static class NoCapCommands {
        public static readonly RoutedUICommand Execute;

        static NoCapCommands() {
            Execute = new RoutedUICommand("E_xecute", "Execute", typeof(NoCapCommands));
        }
    }
}
