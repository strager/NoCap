namespace Bindable.Linq.Aggregators
{
    using Numerics;

    /// <summary>
    /// Aggregates a collection of numeric values into a bindable result, which will be updated when the source
    /// collection changes.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TAverageResult">The type of the average result.</typeparam>
    internal sealed class SumAggregator<TValue, TAverageResult> : Aggregator<TValue, TValue>
    {
        private readonly INumeric<TValue, TAverageResult> _numericHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="SumAggregator&lt;TValue, TAverageResult&gt;"/> class.
        /// </summary>
        /// <param name="sourceCollection">The source collection.</param>
        /// <param name="numericHelper">The numeric helper.</param>
        public SumAggregator(IBindableCollection<TValue> sourceCollection, INumeric<TValue, TAverageResult> numericHelper)
            : base(sourceCollection)
        {
            _numericHelper = numericHelper;
        }

        /// <summary>
        /// When overridden in a derived class, provides the aggregator the opportunity to calculate the
        /// value.
        /// </summary>
        protected override void AggregateOverride()
        {
            Current = _numericHelper.Sum(SourceCollection);
        }
    }
}