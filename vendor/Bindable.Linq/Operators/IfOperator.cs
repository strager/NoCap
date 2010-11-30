namespace Bindable.Linq.Operators
{
    using System;

    /// <summary>
    /// Performs a check against the item, returning the result type depending on whether the item is true or false.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    internal sealed class IfOperator<TSource, TResult> : Operator<TSource, TResult>
    {
        private readonly Func<TSource, bool> _condition;
        private readonly Func<TSource, TResult> _valueIfFalse;
        private readonly Func<TResult> _valueIfNull;
        private readonly Func<TSource, TResult> _valueIfTrue;

        /// <summary>
        /// Initializes a new instance of the <see cref="IfOperator&lt;TSource, TResult&gt;"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="trueValue">The true value.</param>
        /// <param name="falseValue">The false value.</param>
        /// <param name="nullValue">The null value.</param>
        public IfOperator(IBindable<TSource> source, Func<TSource, bool> condition, Func<TSource, TResult> trueValue, Func<TSource, TResult> falseValue, Func<TResult> nullValue)
            : base(source)
        {
            _condition = condition;
            _valueIfTrue = trueValue;
            _valueIfFalse = falseValue;
            _valueIfNull = nullValue;
        }

        /// <summary>
        /// When overridden in a derived class, refreshes the object.
        /// </summary>
        protected override void RefreshOverride()
        {
            var source = Source.Current;
            if (source != null)
            {
                if (_condition(source))
                {
                    Current = _valueIfTrue(source);
                }
                else
                {
                    Current = _valueIfFalse(source);
                }
            }
            else
            {
                Current = _valueIfNull == null ? default(TResult) : _valueIfNull();
            }
        }
    }
}