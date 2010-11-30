namespace Bindable.Linq.Iterators
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The Iterator created when performing a select and projection into another type.
    /// </summary>
    /// <typeparam name="TSource">The type of source item.</typeparam>
    /// <typeparam name="TResult">The type of result item.</typeparam>
    internal sealed class SelectIterator<TSource, TResult> : Iterator<TSource, TResult>
        where TSource : class
    {
        private readonly ProjectionRegister<TSource, TResult> _projectionRegister;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectIterator&lt;T, R&gt;"/> class.
        /// </summary>
        /// <param name="sourceCollection">The source collection.</param>
        /// <param name="projector">The projector.</param>
        public SelectIterator(IBindableCollection<TSource> sourceCollection, Func<TSource, TResult> projector)
            : base(sourceCollection)
        {
            _projectionRegister = new ProjectionRegister<TSource, TResult>(projector);
        }

        /// <summary>
        /// Gets the projection register. This register keeps track of projections from a source type 
        /// to a result type.
        /// </summary>
        /// <value>The projection register.</value>
        public ProjectionRegister<TSource, TResult> ProjectionRegister
        {
            get { return _projectionRegister; }
        }

        /// <summary>
        /// When implemented in a derived class, processes all items in the <see cref="P:SourceCollection"/>.
        /// </summary>
        protected override void LoadSourceCollection()
        {
            ReactToAddRange(0, SourceCollection);
        }

        /// <summary>
        /// When overridden in a derived class, processes an Add event over a range of items.
        /// </summary>
        /// <param name="sourceStartingIndex">Index of the source starting.</param>
        /// <param name="addedItems">The added items.</param>
        protected override void ReactToAddRange(int sourceStartingIndex, IEnumerable<TSource> addedItems)
        {
            var projectedItems = ProjectionRegister.CreateOrGetProjections(addedItems);
            ResultCollection.AddOrInsertRange(sourceStartingIndex, projectedItems);
        }

        /// <summary>
        /// When overridden in a derived class, processes a Move event over a range of items.
        /// </summary>
        /// <param name="sourceStartingIndex">Index of the source starting.</param>
        /// <param name="movedItems">The moved items.</param>
        protected override void ReactToMoveRange(int sourceStartingIndex, IEnumerable<TSource> movedItems)
        {
            var projectedItems = ProjectionRegister.CreateOrGetProjections(movedItems);
            ResultCollection.MoveRange(sourceStartingIndex, projectedItems);
        }

        /// <summary>
        /// When overridden in a derived class, processes a Remove event over a range of items.
        /// </summary>
        /// <param name="removedItems">The removed items.</param>
        protected override void ReactToRemoveRange(IEnumerable<TSource> removedItems)
        {
            var projectedItems = ProjectionRegister.GetProjections(removedItems);
            ResultCollection.RemoveRange(projectedItems);
            ProjectionRegister.RemoveRange(removedItems);
        }

        /// <summary>
        /// When overridden in a derived class, processes a Replace event over a range of items.
        /// </summary>
        /// <param name="oldItems">The old items.</param>
        /// <param name="newItems">The new items.</param>
        protected override void ReactToReplaceRange(IEnumerable<TSource> oldItems, IEnumerable<TSource> newItems)
        {
            ResultCollection.ReplaceRange(ProjectionRegister.GetProjections(oldItems), ProjectionRegister.CreateOrGetProjections(newItems));
            ProjectionRegister.RemoveRange(oldItems);
        }

        /// <summary>
        /// When overridden in a derived class, processes a PropertyChanged event on a source item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="propertyName">Name of the property.</param>
        protected override void ReactToItemPropertyChanged(TSource item, string propertyName)
        {
            var existing = ProjectionRegister.GetExistingProjection(item);
            if (existing is TResult)
            {
                ResultCollection.Replace((TResult) existing, ProjectionRegister.ReProject(item));
            }
        }

        /// <summary>
        /// When overridden in a derived class, provides the derived class with the ability to perform custom actions when
        /// the collection is reset, before the sources are re-loaded.
        /// </summary>
        /// <remarks>Warning: No locks should be held when invoking this method.</remarks>
        protected override void ResetOverride()
        {
            ProjectionRegister.Clear();
            base.ResetOverride();
        }

        /// <summary>
        /// When overridden in a derived class, gives the class an opportunity to dispose any expensive components.
        /// </summary>
        protected override void DisposeOverride()
        {
            ProjectionRegister.Dispose();
        }
    }
}