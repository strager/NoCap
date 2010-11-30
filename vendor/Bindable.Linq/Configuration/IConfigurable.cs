namespace Bindable.Linq.Configuration
{
    /// <summary>
    /// Implemented by classes which can flow a set of Bindable LINQ configuration options between query expressions.
    /// </summary>
    public interface IConfigurable
    {
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        IBindingConfiguration Configuration { get; }
    }
}