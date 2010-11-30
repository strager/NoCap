namespace Bindable.Linq.Aggregators
{
    using System;

    /// <summary>
    /// Aggregates a source collection using a custom accumulator callback provided by the caller.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TAccumulate">The type of the accumulate.</typeparam>
    internal sealed class CustomAggregator<TSource, TAccumulate> : Aggregator<TSource, TAccumulate>
    {
        private readonly Func<TAccumulate, TSource, TAccumulate> _aggregator;
        private readonly TAccumulate _seed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAggregator&lt;TSource, TAccumulate&gt;"/> class.
        /// </summary>
        public CustomAggregator(IBindableCollection<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> aggregator)
            : base(source)
        {
            _aggregator = aggregator;
            _seed = seed;
        }

        /// <summary>
        /// When overridden in a derived class, provides the aggregator the opportunity to calculate the
        /// value.
        /// </summary>
        protected override void AggregateOverride()
        {
            var result = _seed;
            foreach (var sourceItem in SourceCollection)
            {
                result = _aggregator(result, sourceItem);
            }
            Current = result;
        }
    }
}