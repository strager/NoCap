using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoCap.Destinations;

namespace NoCap.Sources {
    public interface ISource {
        bool Get(ISourceResultThing sourceResultThing);
    }

    public interface ISourceResultThing {
        void Start();
        void Done(DestinationType type, object data, string name);
        void Cancelled();
        void Error(string message);
    }
}
