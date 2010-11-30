namespace Bindable.Linq.Dependencies.ExpressionAnalysis.Extractors
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Implemented by objects which analyse expressions and extract dependencies.
    /// </summary>
    public interface IDependencyExtractor
    {
        /// <summary>
        /// Extracts any dependencies within the specified LINQ expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        IEnumerable<IDependencyDefinition> Extract(Expression expression);
    }
}