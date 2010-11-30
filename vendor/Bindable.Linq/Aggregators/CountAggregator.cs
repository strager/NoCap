namespace Bindable.Linq.Aggregators
{
    /// <summary>
    /// An aggregator that returns the count of the items in a Bindable LINQ collection.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    internal sealed class CountAggregator<TSource> : Aggregator<TSource, int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CountAggregator&lt;TSource&gt;"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public CountAggregator(IBindableCollection<TSource> source)
            : base(source) {}

        /// <summary>
        /// When overridden in a derived class, provides the aggregator the opportunity to calculate the
        /// value.
        /// </summary>
        protected override void AggregateOverride()
        {
            Current = SourceCollection.Count;
        }
    }
}