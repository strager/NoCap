﻿using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Processors;

namespace NoCap.Plugins.Factories {
    [Export(typeof(IProcessorFactory))]
    class ClipboardFactory : IProcessorFactory {
        public string Name {
            get { return "Clipboard"; }
        }

        public IProcessor CreateProcessor() {
            return new Clipboard();
        }

        public IProcessorEditor GetProcessorEditor(IProcessor processor) {
            return null;
        }
    }
}
