using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NoCap.Library.Util;

namespace NoCap.Library.Processors {
    public class ProcessorChain : IProcessor, IList<IProcessor> {
        private readonly List<IProcessor> processors = new List<IProcessor>();

        public string Name {
            get { return "Destination chain"; }
        }

        public ProcessorChain() {
        }

        public ProcessorChain(params IProcessor[] processors) :
            this((IEnumerable<IProcessor>) processors) {
        }

        public ProcessorChain(IEnumerable<IProcessor> processors) {
            this.processors.AddRange(processors);
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress) {
            // ToList is needed for some strange reason
            var progressTrackers = this.processors.Select((destination) => new NotifyingProgressTracker()).ToList();
            var aggregateProgress = new AggregateProgressTracker(progressTrackers);
            aggregateProgress.BindTo(progress);

            using (var trackerEnumerator = progressTrackers.GetEnumerator()) {
                foreach (var destination in this.processors) {
                    trackerEnumerator.MoveNext();

                    destination.CheckValidInputType(data);

                    data = destination.Process(data, trackerEnumerator.Current);
                }
            }

            return data;
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            if (!this.processors.Any()) {
                return new TypedDataType[] { };
            }

            return this.processors.First().GetInputDataTypes();
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            if (!this.processors.Any()) {
                return new[] { input };
            }

            this.CheckValidInputType(input);

            return GetChainOutputDataTypes(input, this.processors).Unique();
        }

        private static IEnumerable<TypedDataType> GetChainOutputDataTypes(TypedDataType input, IEnumerable<IProcessor> processors) {
            if (!processors.Any()) {
                return new[] { input };
            }

            var processor = processors.First();

            if (!processor.IsValidInputType(input)) {
                return new TypedDataType[] { };
            }

            var outputTypes = processor.GetOutputDataTypes(input);

            return outputTypes.Aggregate(
                (IEnumerable<TypedDataType>) new TypedDataType[] { },
                (types, type) => types.Concat(GetChainOutputDataTypes(type, processors.Skip(1)))
            );
        }

        public IProcessorFactory GetFactory() {
            return null;
        }

        public IEnumerator<IProcessor> GetEnumerator() {
            return this.processors.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(IProcessor item) {
            this.processors.Add(item);
        }

        public void Clear() {
            this.processors.Clear();
        }

        public bool Contains(IProcessor item) {
            return this.processors.Contains(item);
        }

        public void CopyTo(IProcessor[] array, int arrayIndex) {
            this.processors.CopyTo(array, arrayIndex);
        }

        public bool Remove(IProcessor item) {
            return this.processors.Remove(item);
        }

        public int Count {
            get {
                return this.processors.Count;
            }
        }

        bool ICollection<IProcessor>.IsReadOnly {
            get {
                return false;
            }
        }

        public int IndexOf(IProcessor item) {
            return this.processors.IndexOf(item);
        }

        public void Insert(int index, IProcessor item) {
            this.processors.Insert(index, item);
        }

        public void RemoveAt(int index) {
            this.processors.RemoveAt(index);
        }

        public IProcessor this[int index] {
            get {
                return this.processors[index];
            }

            set {
                this.processors[index] = value;
            }
        }
    }
}
