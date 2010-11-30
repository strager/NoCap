namespace Bindable.Linq.Dependencies.ExpressionAnalysis.Extractors
{
    using System.Linq.Expressions;
    using Definitions;

    /// <summary>
    /// Extracts dependencies placed on child items within a query by looking for uses of <see cref="ParameterExpression"/>.
    /// </summary>
    internal sealed class ItemDependencyExtractor : DependencyExtractor
    {
        /// <summary>
        /// When overridden in a derived class, extracts the appropriate dependency from the root of the expression.
        /// </summary>
        /// <param name="rootExpression">The root expression.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <returns></returns>
        protected override IDependencyDefinition ExtractFromRoot(Expression rootExpression, string propertyPath)
        {
            IDependencyDefinition result = null;
            if (rootExpression is ParameterExpression)
            {
                var parameterExpression = (ParameterExpression) rootExpression;
                result = new ItemDependencyDefinition(propertyPath, parameterExpression.Name);
            }
            return result;
        }
    }
}