using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace NoCap.Library {
    public static class NoCapCommands {
        public static readonly RoutedUICommand Execute;
        public static readonly RoutedUICommand Cancel;

        static NoCapCommands() {
            Execute = new RoutedUICommand("E_xecute", "Execute", typeof(NoCapCommands));
            Cancel  = new RoutedUICommand("_Cancel",  "Cancel",  typeof(NoCapCommands));
        }
    }
}
