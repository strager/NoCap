namespace Bindable.Linq.Operators
{
    using System;

    /// <summary>
    /// Projects one item to another item.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    internal sealed class ProjectOperator<TSource, TResult> : Operator<TSource, TResult>
    {
        private readonly Func<TSource, TResult> _projector;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectOperator&lt;TSource, TResult&gt;"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="projector">The projector.</param>
        public ProjectOperator(IBindable<TSource> source, Func<TSource, TResult> projector)
            : base(source)
        {
            _projector = projector;
        }

        /// <summary>
        /// When overridden in a derived class, refreshes the object.
        /// </summary>
        protected override void RefreshOverride()
        {
            var source = Source.Current;
            if (source != null)
            {
                Current = _projector(source);
            }
            else
            {
                Current = default(TResult);
            }
        }
    }
}