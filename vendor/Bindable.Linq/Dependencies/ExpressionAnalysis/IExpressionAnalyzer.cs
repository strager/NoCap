namespace Bindable.Linq.Dependencies.ExpressionAnalysis
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Implemented by classes that can parse LINQ expression trees and extract dependencies from them.
    /// </summary>
    public interface IExpressionAnalyzer
    {
        /// <summary>
        /// Extracts the dependencies.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="itemParameter">The item parameter.</param>
        IEnumerable<IDependencyDefinition> DiscoverDependencies(Expression expression, ParameterExpression itemParameter);
    }
}