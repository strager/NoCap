namespace Bindable.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Windows;
    using System.Windows.Threading;
    using Adapters;
    using Aggregators;
    using Bindable.Linq.Aggregators.Numerics;
    using Bindable.Linq.Dependencies.Definitions;
    using Collections;
    using Configuration;
    using Dependencies;
    using Helpers;
    using Iterators;
    using Operators;
    using Threading;

    /// <summary>
    /// This class contains all of the extension method implementations provided by Bindable LINQ. 
    /// </summary>
    public static class Extensions
    {
        #region Iterators

        #region AsBindable (DONE)
        /// <summary>
        /// Converts any <see cref="T:IEnumerable`1"/> into a Bindable LINQ <see cref="T:ISyncLinqCollection`1"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of source item.</typeparam>
        /// <param name="source">The source Iterator.</param>
        /// <returns>
        /// An <see cref="T:ISyncLinqCollection`1"/> containing the items.
        /// </returns>
        /// <exception cref="T:ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null.</exception>
        public static IBindableCollection<TSource> AsBindable<TSource>(this IEnumerable source) where TSource : class
        {
            return source.AsBindable<TSource>(BindingConfigurations.Default);
        }

        /// <summary>
        /// Converts any <see cref="T:IEnumerable`1"/> into a Bindable LINQ <see cref="T:ISyncLinqCollection`1"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of source item.</typeparam>
        /// <param name="source">The source Iterator.</param>
        /// <param name="bindingConfiguration">The binding configuration.</param>
        /// <returns>
        /// An <see cref="T:ISyncLinqCollection`1"/> containing the items.
        /// </returns>
        /// <exception cref="T:ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null.</exception>
        public static IBindableCollection<TSource> AsBindable<TSource>(this IEnumerable source, IBindingConfiguration bindingConfiguration) where TSource : class
        {
            source.ShouldNotBeNull("source");
            bindingConfiguration.ShouldNotBeNull("bindingConfiguration");

            var alreadyBindable = source as IBindableCollection<TSource>;
            if (alreadyBindable != null)
            {
                return alreadyBindable;
            }
            return new BindableCollectionAdapter<TSource>(source, true, bindingConfiguration);
        }

        /// <summary>
        /// Converts any <see cref="T:IEnumerable`1"/> into a Bindable LINQ <see cref="T:ISyncLinqCollection`1"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of source item.</typeparam>
        /// <param name="source">The source Iterator.</param>
        /// <param name="bindingConfiguration">The binding configuration.</param>
        /// <returns>
        /// An <see cref="T:ISyncLinqCollection`1"/> containing the items.
        /// </returns>
        /// <exception cref="T:ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null.</exception>
        public static IBindableCollection<TSource> AsBindable<TSource>(this IEnumerable<TSource> source, IBindingConfiguration bindingConfiguration) where TSource : class
        {
            return ((IEnumerable)source).AsBindable<TSource>(bindingConfiguration);
        }

        /// <summary>
        /// Converts any <see cref="T:IEnumerable`1"/> into a Bindable LINQ <see cref="T:ISyncLinqCollection`1"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of source item.</typeparam>
        /// <param name="source">The source Iterator.</param>
        /// <returns>
        /// An <see cref="T:ISyncLinqCollection`1"/> containing the items.
        /// </returns>
        /// <exception cref="T:ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null.</exception>
        public static IBindableCollection<TSource> AsBindable<TSource>(this IEnumerable<TSource> source) where TSource : class
        {
            return ((IEnumerable)source).AsBindable<TSource>();
        }

        /// <summary>
        /// Converts any <see cref="T:IEnumerable`1"/> into a Bindable LINQ <see cref="T:ISyncLinqCollection`1"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of source item.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source Iterator.</param>
        /// <returns>
        /// An <see cref="T:ISyncLinqCollection`1"/> containing the items.
        /// </returns>
        /// <exception cref="T:ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null.</exception>
        public static IBindableQuery<TResult> AsBindable<TSource, TResult>(this IEnumerable<TSource> source)
            where TResult : TSource
            where TSource : class
        {
            source.ShouldNotBeNull("source");
            return new SelectIterator<TSource, TResult>(source.AsBindable(), i => (TResult)i);
        }
        #endregion

        #region Asynchronous (DONE)
#if !SILVERLIGHT
        /// <summary>
        /// Converts any <see cref="T:IEnumerable`1"/> into a Bindable LINQ <see cref="T:ISyncLinqCollection`1"/>, 
        /// and executes the enumerator for the source collection on a background thread. 
        /// </summary>
        /// <typeparam name="TSource">The type of source item.</typeparam>
        /// <param name="source">The source Iterator.</param>
        /// <returns>
        /// A <see cref="T:ISyncLinqCollection`1"/> containing the items, which will be added 
        /// asynchronously.
        /// </returns>
        /// <exception cref="T:ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null.</exception>
        public static IBindableQuery<TSource> Asynchronous<TSource>(this IBindableCollection<TSource> source) where TSource : class
        {
            return source.Asynchronous(DispatcherFactory.Create(Dispatcher.CurrentDispatcher));
        }
#endif

        /// <summary>
        /// Converts any <see cref="T:IEnumerable`1"/> into a Bindable LINQ <see cref="T:ISyncLinqCollection`1"/>,
        /// and executes the enumerator for the source collection on a background thread.
        /// </summary>
        /// <typeparam name="TSource">The type of source item.</typeparam>
        /// <param name="source">The source Iterator.</param>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <returns>
        /// A <see cref="T:ISyncLinqCollection`1"/> containing the items, which will be added
        /// asynchronously.
        /// </returns>
        /// <exception cref="T:ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null.</exception>
        public static IBindableQuery<TSource> Asynchronous<TSource>(this IBindableCollection<TSource> source, Dispatcher dispatcher) where TSource : class
        {
            return source.Asynchronous(DispatcherFactory.Create(dispatcher));
        }

        /// <summary>
        /// Converts any <see cref="T:IEnumerable`1"/> into a Bindable LINQ <see cref="T:ISyncLinqCollection`1"/>,
        /// and executes the enumerator for the source collection on a background thread.
        /// </summary>
        /// <typeparam name="TSource">The type of source item.</typeparam>
        /// <param name="source">The source Iterator.</param>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <returns>
        /// A <see cref="T:ISyncLinqCollection`1"/> containing the items, which will be added
        /// asynchronously.
        /// </returns>
        /// <exception cref="T:ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null.</exception>
        public static IBindableQuery<TSource> Asynchronous<TSource>(this IBindableCollection<TSource> source, IDispatcher dispatcher) where TSource : class
        {
            source.ShouldNotBeNull("source");
            return new AsynchronousIterator<TSource>(source, dispatcher);
        }
        #endregion

        #region Cast (DONE)
        /// <summary>
        /// Converts the elements of an <see cref="T:IBindableCollection"/> to the specified type.
        /// </summary>
        /// <typeparam name="TResult">The type to convert the elements of <paramref name="source"/> to.</typeparam>
        /// <param name="source">The <see cref="T:IBindableCollection"/> that contains the elements to be converted.</param>
        /// <returns>
        /// An <see cref="T:IBindableCollection`1"/> that contains each element of the source sequence converted to the specified type.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="source"/> is null.</exception>
        /// <exception cref="T:System.InvalidCastException">An element in the sequence cannot be cast to type <paramref name="TResult"/>.</exception>
        public static IBindableCollection<TResult> Cast<TResult>(this IBindableCollection source) where TResult : class
        {
            return AsBindable<TResult>(source);
        }
        #endregion

        #region Concat (DONE)
        /// <summary>Concatenates two sequences.</summary>
        /// <returns>An <see cref="T:IBindableCollection`1" /> that contains the concatenated elements of the two input sequences.</returns>
        /// <param name="first">The first sequence to concatenate.</param>
        /// <param name="second">The sequence to concatenate to the first sequence.</param>
        /// <typeparam name="TElement">The type of the elements of the input sequences.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="first" /> or <paramref name="second" /> is null.</exception>
        public static IBindableQuery<TElement> Concat<TElement>(this IBindableCollection<TElement> first, IBindableCollection<TElement> second) where TElement : class
        {
            return Union(first, second);
        }
        #endregion

        #region Distinct (DONE)
        /// <summary>Returns distinct elements from a sequence by using the default equality comparer to compare values.</summary>
        /// <returns>An <see cref="T:IBindableCollection`1" /> that contains distinct elements from the source sequence.</returns>
        /// <param name="source">The sequence to remove duplicate elements from.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindableQuery<TSource> Distinct<TSource>(this IBindableCollection<TSource> source) where TSource : class
        {
            return source.Distinct(null);
        }

        /// <summary>Returns distinct elements from a sequence by using a specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare values.</summary>
        /// <returns>An <see cref="T:IBindableCollection`1" /> that contains distinct elements from the source sequence.</returns>
        /// <param name="source">The sequence to remove duplicate elements from.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare values.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindableQuery<TSource> Distinct<TSource>(this IBindableCollection<TSource> source, IEqualityComparer<TSource> comparer) where TSource : class
        {
            if (comparer == null)
            {
                comparer = new DefaultComparer<TSource>();
            }
            return source.GroupBy(c => comparer.GetHashCode(c)).Select(group => group.First().Current);
        }
        #endregion

        #region Except (NOT)
        /// <summary>Produces the set difference of two sequences by using the default equality comparer to compare values.</summary>
        /// <returns>A sequence that contains the set difference of the elements of two sequences.</returns>
        /// <param name="first">An <see cref="T:IBindableCollection`1" /> whose elements that are not also in <paramref name="second" /> will be returned.</param>
        /// <param name="second">An <see cref="T:IBindableCollection`1" /> whose elements that also occur in the first sequence will cause those elements to be removed from the returned sequence.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="first" /> or <paramref name="second" /> is null.</exception>
        public static IBindableQuery<TSource> Except<TSource>(this IBindableCollection<TSource> first, IEnumerable<TSource> second)
        {
            return first.Except(second, null);
        }

        /// <summary>Produces the set difference of two sequences by using the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare values.</summary>
        /// <returns>A sequence that contains the set difference of the elements of two sequences.</returns>
        /// <param name="first">An <see cref="T:IBindableCollection`1" /> whose elements that are not also in <paramref name="second" /> will be returned.</param>
        /// <param name="second">An <see cref="T:IBindableCollection`1" /> whose elements that also occur in the first sequence will cause those elements to be removed from the returned sequence.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare values.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="first" /> or <paramref name="second" /> is null.</exception>
        public static IBindableQuery<TSource> Except<TSource>(this IBindableCollection<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (comparer == null)
            {
                comparer = new DefaultComparer<TSource>();
            }
            first.ShouldNotBeNull("first");
            second.ShouldNotBeNull("second");
            throw new NotImplementedException();
        }
        #endregion

        #region GroupBy (DONE)
        /// <summary>Groups the elements of a sequence according to a specified key selector function.</summary>
        /// <returns>An IEnumerable&lt;IGrouping&lt;TKey, TSource&gt;&gt; in C# or IEnumerable(Of IGrouping(Of TKey, TSource)) in Visual Basic where each <see cref="T:System.Linq.IGrouping`2" /> object contains a sequence of objects and a key.</returns>
        /// <param name="source">An <see cref="T:IBindableCollection`1" /> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> is null.</exception>
        public static IBindableQuery<IBindableGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IBindableCollection<TSource> source, Expression<Func<TSource, TKey>> keySelector) where TSource : class
        {
            return source.GroupBy(keySelector, s => s, new DefaultComparer<TKey>());
        }

        /// <summary>Groups the elements of a sequence according to a specified key selector function and compares the keys by using a specified comparer.</summary>
        /// <returns>An IEnumerable&lt;IGrouping&lt;TKey, TSource&gt;&gt; in C# or IEnumerable(Of IGrouping(Of TKey, TSource)) in Visual Basic where each <see cref="T:System.Linq.IGrouping`2" /> object contains a collection of objects and a key.</returns>
        /// <param name="source">An <see cref="T:IBindableCollection`1" /> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare keys.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> is null.</exception>
        public static IBindableQuery<IBindableGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IBindableCollection<TSource> source, Expression<Func<TSource, TKey>> keySelector, IEqualityComparer<TKey> comparer) where TSource : class
        {
            return source.GroupBy(keySelector, s => s, comparer);
        }

        /// <summary>Groups the elements of a sequence according to a specified key selector function and projects the elements for each group by using a specified function.</summary>
        /// <returns>An IEnumerable&lt;IGrouping&lt;TKey, TElement&gt;&gt; in C# or IEnumerable(Of IGrouping(Of TKey, TElement)) in Visual Basic where each <see cref="T:System.Linq.IGrouping`2" /> object contains a collection of objects of type <paramref name="TElement" /> and a key.</returns>
        /// <param name="source">An <see cref="T:IBindableCollection`1" /> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="elementSelector">A function to map each source element to an element in the <see cref="T:System.Linq.IGrouping`2" />.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
        /// <typeparam name="TElement">The type of the elements in the <see cref="T:System.Linq.IGrouping`2" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="elementSelector" /> is null.</exception>
        public static IBindableQuery<IBindableGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IBindableCollection<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector)
            where TSource : class
            where TElement : class
        {
            return source.GroupBy(keySelector, elementSelector, null);
        }

        /// <summary>Groups the elements of a sequence according to a specified key selector function and creates a result value from each group and its key.</summary>
        /// <returns>A collection of elements of type <paramref name="TResult" /> where each element represents a projection over a group and its key.</returns>
        /// <param name="source">An <see cref="T:IBindableCollection`1" /> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="resultSelector">A function to create a result value from each group.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
        /// <typeparam name="TResult">The type of the result value returned by <paramref name="resultSelector" />.</typeparam>
        public static IBindableQuery<TResult> GroupBy<TSource, TKey, TResult>(this IBindableCollection<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TKey, IBindableCollection<TSource>, TResult>> resultSelector)
            where TSource : class
            where TResult : class
        {
            return source.GroupBy(keySelector, s => s, new DefaultComparer<TKey>()).Into(resultSelector);
        }

        /// <summary>Groups the elements of a sequence according to a key selector function. The keys are compared by using a comparer and each group's elements are projected by using a specified function.</summary>
        /// <returns>An IEnumerable&lt;IGrouping&lt;TKey, TElement&gt;&gt; in C# or IEnumerable(Of IGrouping(Of TKey, TElement)) in Visual Basic where each <see cref="T:System.Linq.IGrouping`2" /> object contains a collection of objects of type <paramref name="TElement" /> and a key.</returns>
        /// <param name="source">An <see cref="T:IBindableCollection`1" /> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="elementSelector">A function to map each source element to an element in an <see cref="T:System.Linq.IGrouping`2" />.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare keys.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
        /// <typeparam name="TElement">The type of the elements in the <see cref="T:System.Linq.IGrouping`2" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="elementSelector" /> is null.</exception>
        public static IBindableQuery<IBindableGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IBindableCollection<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector, IEqualityComparer<TKey> comparer)
            where TSource : class
            where TElement : class
        {
            source.ShouldNotBeNull("source");
            keySelector.ShouldNotBeNull("keySelector");
            elementSelector.ShouldNotBeNull("elementSelector");
            return new GroupByIterator<TKey, TSource, TElement>(source, keySelector, elementSelector, comparer).WithDependencyExpression(keySelector.Body, keySelector.Parameters[0]);
        }

        /// <summary>Groups the elements of a sequence according to a specified key selector function and creates a result value from each group and its key. The elements of each group are projected by using a specified function.</summary>
        /// <returns>A collection of elements of type <paramref name="TResult" /> where each element represents a projection over a group and its key.</returns>
        /// <param name="source">An <see cref="T:IBindableCollection`1" /> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="elementSelector">A function to map each source element to an element in an <see cref="T:System.Linq.IGrouping`2" />.</param>
        /// <param name="resultSelector">A function to create a result value from each group.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
        /// <typeparam name="TElement">The type of the elements in each <see cref="T:System.Linq.IGrouping`2" />.</typeparam>
        /// <typeparam name="TResult">The type of the result value returned by <paramref name="resultSelector" />.</typeparam>
        public static IBindableQuery<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IBindableCollection<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector, Expression<Func<TKey, IBindableCollection<TElement>, TResult>> resultSelector)
            where TSource : class
            where TElement : class
            where TResult : class
        {
            return source.GroupBy(keySelector, elementSelector, null).Into(resultSelector);
        }

        /// <summary>Groups the elements of a sequence according to a specified key selector function and creates a result value from each group and its key. The keys are compared by using a specified comparer.</summary>
        /// <returns>A collection of elements of type <paramref name="TResult" /> where each element represents a projection over a group and its key.</returns>
        /// <param name="source">An <see cref="T:IBindableCollection`1" /> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="resultSelector">A function to create a result value from each group.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare keys with.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
        /// <typeparam name="TResult">The type of the result value returned by <paramref name="resultSelector" />.</typeparam>
        public static IBindableQuery<TResult> GroupBy<TSource, TKey, TResult>(this IBindableCollection<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TKey, IBindableCollection<TSource>, TResult>> resultSelector, IEqualityComparer<TKey> comparer)
            where TSource : class
            where TResult : class
        {
            return source.GroupBy(keySelector, s => s, comparer).Into(resultSelector);
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function and creates a result value from each group and its key. Key values are compared by using a specified comparer, and the elements of each group are projected by using a specified function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <typeparam name="TElement">The type of the elements in each <see cref="T:System.Linq.IGrouping`2"/>.</typeparam>
        /// <typeparam name="TResult">The type of the result value returned by <paramref name="resultSelector"/>.</typeparam>
        /// <param name="source">An <see cref="T:IBindableCollection`1"/> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="elementSelector">A function to map each source element to an element in an <see cref="T:System.Linq.IGrouping`2"/>.</param>
        /// <param name="resultSelector">A function to create a result value from each group.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> to compare keys with.</param>
        /// <returns>
        /// A collection of elements of type <paramref name="TResult"/> where each element represents a projection over a group and its key.
        /// </returns>
        public static IBindableQuery<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IBindableCollection<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector, Expression<Func<TKey, IBindableCollection<TElement>, TResult>> resultSelector, IEqualityComparer<TKey> comparer)
            where TSource : class
            where TElement : class
            where TResult : class
        {
            return source.GroupBy(keySelector, elementSelector, comparer).Into(resultSelector);
        }
        #endregion

        #region Into (DONE)
        /// <summary>
        /// Projects the groups from a GroupBy into a new element type.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <typeparam name="TElement">The type of the elements in each <see cref="T:System.Linq.IGrouping`2"/>.</typeparam>
        /// <typeparam name="TResult">The type of the result value returned by <paramref name="resultSelector"/>.</typeparam>
        /// <param name="source">An <see cref="T:IBindableCollection`1"/> whose elements to group.</param>
        /// <param name="resultSelector">A function to create a result value from each group.</param>
        /// <returns>
        /// A collection of elements of type <paramref name="TResult"/> where each element represents a projection over a group and its key.
        /// </returns>
        public static IBindableQuery<TResult> Into<TKey, TElement, TResult>(this IBindableQuery<IBindableGrouping<TKey, TElement>> source, Expression<Func<TKey, IBindableCollection<TElement>, TResult>> resultSelector)
            where TElement : class
            where TResult : class
        {
            var func = resultSelector.Compile();
            return source.Select(g => func(g.Key, g));
        }
        #endregion

        #region GroupJoin (NOT)
        /// <summary>Correlates the elements of two sequences based on equality of keys and groups the results. The default equality comparer is used to compare keys.</summary>
        /// <returns>An <see cref="T:IBindableCollection`1" /> that contains elements of type <paramref name="TResult" /> that are obtained by performing a grouped join on two sequences.</returns>
        /// <param name="outer">The first sequence to join.</param>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from an element from the first sequence and a collection of matching elements from the second sequence.</param>
        /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="outer" /> or <paramref name="inner" /> or <paramref name="outerKeySelector" /> or <paramref name="innerKeySelector" /> or <paramref name="resultSelector" /> is null.</exception>
        public static IBindableQuery<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IBindableCollection<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector)
            where TOuter : class
            where TInner : class
            where TResult : class
        {
            throw new NotImplementedException();
        }

        /// <summary>Correlates the elements of two sequences based on key equality and groups the results. A specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> is used to compare keys.</summary>
        /// <returns>An <see cref="T:IBindableCollection`1" /> that contains elements of type <paramref name="TResult" /> that are obtained by performing a grouped join on two sequences.</returns>
        /// <param name="outer">The first sequence to join.</param>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from an element from the first sequence and a collection of matching elements from the second sequence.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to hash and compare keys.</param>
        /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="outer" /> or <paramref name="inner" /> or <paramref name="outerKeySelector" /> or <paramref name="innerKeySelector" /> or <paramref name="resultSelector" /> is null.</exception>
        public static IBindableQuery<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IBindableCollection<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector, IEqualityComparer<TKey> comparer)
            where TOuter : class
            where TInner : class
            where TResult : class
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Intersect (NOT)
        /// <summary>Produces the set intersection of two sequences by using the default equality comparer to compare values.</summary>
        /// <returns>A sequence that contains the elements that form the set intersection of two sequences.</returns>
        /// <param name="first">An <see cref="T:IBindableCollection`1" /> whose distinct elements that also appear in <paramref name="second" /> will be returned.</param>
        /// <param name="second">An <see cref="T:IBindableCollection`1" /> whose distinct elements that also appear in the first sequence will be returned.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="first" /> or <paramref name="second" /> is null.</exception>
        public static IBindableQuery<TSource> Intersect<TSource>(this IBindableCollection<TSource> first, IEnumerable<TSource> second) where TSource : class
        {
            throw new NotImplementedException();
        }

        /// <summary>Produces the set intersection of two sequences by using the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare values.</summary>
        /// <returns>A sequence that contains the elements that form the set intersection of two sequences.</returns>
        /// <param name="first">An <see cref="T:IBindableCollection`1" /> whose distinct elements that also appear in <paramref name="second" /> will be returned.</param>
        /// <param name="second">An <see cref="T:IBindableCollection`1" /> whose distinct elements that also appear in the first sequence will be returned.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare values.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="first" /> or <paramref name="second" /> is null.</exception>
        public static IBindableQuery<TSource> Intersect<TSource>(this IBindableCollection<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer) where TSource : class
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Join (NOT)
        /// <summary>Correlates the elements of two sequences based on matching keys. The default equality comparer is used to compare keys.</summary>
        /// <returns>An <see cref="T:IBindableCollection`1" /> that has elements of type <paramref name="TResult" /> that are obtained by performing an inner join on two sequences.</returns>
        /// <param name="outer">The first sequence to join.</param>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from two matching elements.</param>
        /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="outer" /> or <paramref name="inner" /> or <paramref name="outerKeySelector" /> or <paramref name="innerKeySelector" /> or <paramref name="resultSelector" /> is null.</exception>
        public static IBindableCollection<TResult> Join<TOuter, TInner, TKey, TResult>(this IBindableCollection<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector)
            where TOuter : class
            where TInner : class
            where TResult : class
        {
            throw new NotImplementedException();
        }

        /// <summary>Correlates the elements of two sequences based on matching keys. A specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> is used to compare keys.</summary>
        /// <returns>An <see cref="T:IBindableCollection`1" /> that has elements of type <paramref name="TResult" /> that are obtained by performing an inner join on two sequences.</returns>
        /// <param name="outer">The first sequence to join.</param>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from two matching elements.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to hash and compare keys.</param>
        /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="outer" /> or <paramref name="inner" /> or <paramref name="outerKeySelector" /> or <paramref name="innerKeySelector" /> or <paramref name="resultSelector" /> is null.</exception>
        public static IBindableCollection<TResult> Join<TOuter, TInner, TKey, TResult>(this IBindableCollection<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector, IEqualityComparer<TKey> comparer)
            where TOuter : class
            where TInner : class
            where TResult : class
        {
            throw new NotImplementedException();
        }
        #endregion

        #region OfType (DONE)
        /// <summary>Filters the elements of an <see cref="T:IBindableCollection" /> based on a specified type.</summary>
        /// <returns>An <see cref="T:IBindableCollection`1" /> that contains elements from the input sequence of type <paramref name="TResult" />.</returns>
        /// <param name="source">The <see cref="T:IBindableCollection" /> whose elements to filter.</param>
        /// <typeparam name="TResult">The type to filter the elements of the sequence on.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindableCollection<TResult> OfType<TResult>(this IBindableCollection source) where TResult : class
        {
            source.ShouldNotBeNull("source");
            var configuration = BindingConfigurations.Default;
            if (source is IConfigurable)
            {
                configuration = ((IConfigurable)source).Configuration;
            }

            return new BindableCollectionAdapter<TResult>(source, false, configuration);
        }
        #endregion

        #region OrderBy (DONE)
        /// <summary>Sorts the elements of a sequence in ascending order according to a key.</summary>
        /// <returns>An <see cref="T:System.Linq.IOrderedEnumerable`1" /> whose elements are sorted according to a key.</returns>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> is null.</exception>
        public static IOrderedBindableQuery<TSource> OrderBy<TSource, TKey>(this IBindableCollection<TSource> source, Expression<Func<TSource, TKey>> keySelector) where TSource : class
        {
            return source.OrderBy(keySelector, null);
        }

        /// <summary>Sorts the elements of a sequence in ascending order by using a specified comparer.</summary>
        /// <returns>An <see cref="T:System.Linq.IOrderedEnumerable`1" /> whose elements are sorted according to a key.</returns>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IComparer`1" /> to compare keys.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> is null.</exception>
        public static IOrderedBindableQuery<TSource> OrderBy<TSource, TKey>(this IBindableCollection<TSource> source, Expression<Func<TSource, TKey>> keySelector, IComparer<TKey> comparer) where TSource : class
        {
            source.ShouldNotBeNull("source");
            keySelector.ShouldNotBeNull("keySelector");
            return new OrderByIterator<TSource, TKey>(source, new ItemSorter<TSource, TKey>(null, keySelector.Compile(), comparer, true)).WithDependencyExpression(keySelector.Body, keySelector.Parameters[0]);
        }
        #endregion

        #region OrderByDescending (DONE)
        /// <summary>Sorts the elements of a sequence in descending order according to a key.</summary>
        /// <returns>An <see cref="T:System.Linq.IOrderedEnumerable`1" /> whose elements are sorted in descending order according to a key.</returns>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> is null.</exception>
        public static IOrderedBindableQuery<TSource> OrderByDescending<TSource, TKey>(this IBindableCollection<TSource> source, Expression<Func<TSource, TKey>> keySelector) where TSource : class
        {
            return source.OrderByDescending(keySelector, null);
        }

        /// <summary>Sorts the elements of a sequence in descending order by using a specified comparer.</summary>
        /// <returns>An <see cref="T:System.Linq.IOrderedEnumerable`1" /> whose elements are sorted in descending order according to a key.</returns>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IComparer`1" /> to compare keys.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> is null.</exception>
        public static IOrderedBindableQuery<TSource> OrderByDescending<TSource, TKey>(this IBindableCollection<TSource> source, Expression<Func<TSource, TKey>> keySelector, IComparer<TKey> comparer) where TSource : class
        {
            source.ShouldNotBeNull("source");
            keySelector.ShouldNotBeNull("keySelector");
            return new OrderByIterator<TSource, TKey>(source, new ItemSorter<TSource, TKey>(null, keySelector.Compile(), comparer, false)).WithDependencyExpression(keySelector.Body, keySelector.Parameters[0]);
        }
        #endregion

        #region Polling (DONE)
#if !SILVERLIGHT

        /// <summary>
        /// Converts any <see cref="T:IEnumerable`1"/> into a Bindable LINQ <see cref="T:ISyncLinqCollection`1"/>. 
        /// Bindable LINQ will automatically poll the collection for changes everytime a given 
        /// timespan elapses.
        /// </summary>
        /// <typeparam name="TSource">The type of source item.</typeparam>
        /// <param name="source">The source Iterator.</param>
        /// <param name="time">The time to wait between polling.</param>
        /// <returns>A <see cref="T:ISyncLinqCollection`1"/> containing the items.</returns>
        public static IBindableQuery<TSource> Polling<TSource>(this IBindableCollection<TSource> source, TimeSpan time) where TSource : class
        {
            return source.Polling(DispatcherFactory.Create(Dispatcher.CurrentDispatcher), time);
        }

#endif

        /// <summary>
        /// Converts any <see cref="T:IEnumerable`1"/> into a Bindable LINQ <see cref="T:ISyncLinqCollection`1"/>.
        /// Bindable LINQ will automatically poll the collection for changes everytime a given timespan elapses.
        /// </summary>
        /// <typeparam name="TSource">The type of source item.</typeparam>
        /// <param name="source">The source Iterator.</param>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <param name="time">The time to wait between polling.</param>
        /// <returns>
        /// A <see cref="T:ISyncLinqCollection`1"/> containing the items.
        /// </returns>
        public static IBindableQuery<TSource> Polling<TSource>(this IBindableCollection<TSource> source, Dispatcher dispatcher, TimeSpan time) where TSource : class
        {
            return source.Polling(DispatcherFactory.Create(dispatcher), time);
        }

        /// <summary>
        /// Converts any <see cref="T:IEnumerable`1"/> into a Bindable LINQ <see cref="T:ISyncLinqCollection`1"/>.
        /// Bindable LINQ will automatically poll the collection for changes everytime a given
        /// timespan elapses.
        /// </summary>
        /// <typeparam name="TSource">The type of source item.</typeparam>
        /// <param name="source">The source Iterator.</param>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <param name="time">The time to wait between polling.</param>
        /// <returns>
        /// A <see cref="T:ISyncLinqCollection`1"/> containing the items.
        /// </returns>
        public static IBindableQuery<TSource> Polling<TSource>(this IBindableCollection<TSource> source, IDispatcher dispatcher, TimeSpan time) where TSource : class
        {
            dispatcher.ShouldNotBeNull("dispatcher");
            source.ShouldNotBeNull("source");
            return new PollIterator<TSource>(source, dispatcher, time);
        }
        #endregion

        #region Select (DONE)
        /// <summary>Projects each element of a sequence into a new form.</summary>
        /// <returns>An <see cref="T:IBindableCollection`1" /> whose elements are the result of invoking the transform function on each element of <paramref name="source" />.</returns>
        /// <param name="source">A sequence of values to invoke a transform function on.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IBindableQuery<TSource> Select<TSource>(this IBindableCollection<TSource> source) where TSource : class
        {
            return source.Select(s => s);
        }

        /// <summary>Projects each element of a sequence into a new form.</summary>
        /// <returns>An <see cref="T:IBindableCollection`1" /> whose elements are the result of invoking the transform function on each element of <paramref name="source" />.</returns>
        /// <param name="source">A sequence of values to invoke a transform function on.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by <paramref name="selector" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IBindableQuery<TResult> Select<TSource, TResult>(this IBindableCollection<TSource> source, Expression<Func<TSource, TResult>> selector) where TSource : class
        {
            source.ShouldNotBeNull("source");
            selector.ShouldNotBeNull("selector");
            return new SelectIterator<TSource, TResult>(source, selector.Compile()).WithDependencyExpression(selector.Body, selector.Parameters[0]);
        }
        #endregion

        #region SelectMany (DONE)
        /// <summary>Projects each element of a sequence to an <see cref="T:IBindableCollection`1" /> and flattens the resulting sequences into one sequence.</summary>
        /// <returns>An <see cref="T:IBindableCollection`1" /> whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.</returns>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the sequence returned by <paramref name="selector" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IBindableQuery<TResult> SelectMany<TSource, TResult>(this IBindableCollection<TSource> source, Expression<Func<TSource, IBindableCollection<TResult>>> selector)
            where TSource : class
            where TResult : class
        {
            source.ShouldNotBeNull("source");
            return source.Select(selector).UnionAll();
        }
        #endregion

        #region ThenBy (DONE)
        /// <summary>Performs a subsequent ordering of the elements in a sequence in ascending order according to a key.</summary>
        /// <returns>An <see cref="T:System.Linq.IOrderedEnumerable`1" /> whose elements are sorted according to a key.</returns>
        /// <param name="source">An <see cref="T:System.Linq.IOrderedEnumerable`1" /> that contains elements to sort.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> is null.</exception>
        public static IOrderedBindableQuery<TSource> ThenBy<TSource, TKey>(this IOrderedBindableQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector) where TSource : class
        {
            return source.ThenBy(keySelector, null);
        }

        /// <summary>Performs a subsequent ordering of the elements in a sequence in ascending order by using a specified comparer.</summary>
        /// <returns>An <see cref="T:System.Linq.IOrderedEnumerable`1" /> whose elements are sorted according to a key.</returns>
        /// <param name="source">An <see cref="T:System.Linq.IOrderedEnumerable`1" /> that contains elements to sort.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IComparer`1" /> to compare keys.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> is null.</exception>
        public static IOrderedBindableQuery<TSource> ThenBy<TSource, TKey>(this IOrderedBindableQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector, IComparer<TKey> comparer) where TSource : class
        {
            source.ShouldNotBeNull("source");
            keySelector.ShouldNotBeNull("keySelector");
            return source.CreateOrderedIterator(keySelector.Compile(), comparer, false).WithDependencyExpression(keySelector.Body, keySelector.Parameters[0]);
        }
        #endregion

        #region ThenByDescending (DONE)
        /// <summary>Performs a subsequent ordering of the elements in a sequence in descending order, according to a key.</summary>
        /// <returns>An <see cref="T:System.Linq.IOrderedEnumerable`1" /> whose elements are sorted in descending order according to a key.</returns>
        /// <param name="source">An <see cref="T:System.Linq.IOrderedEnumerable`1" /> that contains elements to sort.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> is null.</exception>
        public static IOrderedBindableQuery<TSource> ThenByDescending<TSource, TKey>(this IOrderedBindableQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector) where TSource : class
        {
            return source.ThenByDescending(keySelector, null);
        }

        /// <summary>Performs a subsequent ordering of the elements in a sequence in descending order by using a specified comparer.</summary>
        /// <returns>An <see cref="T:System.Linq.IOrderedEnumerable`1" /> whose elements are sorted in descending order according to a key.</returns>
        /// <param name="source">An <see cref="T:System.Linq.IOrderedEnumerable`1" /> that contains elements to sort.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IComparer`1" /> to compare keys.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="keySelector" /> is null.</exception>
        public static IOrderedBindableQuery<TSource> ThenByDescending<TSource, TKey>(this IOrderedBindableQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector, IComparer<TKey> comparer) where TSource : class
        {
            source.ShouldNotBeNull("source");
            keySelector.ShouldNotBeNull("keySelector");
            return source.CreateOrderedIterator(keySelector.Compile(), comparer, true).WithDependencyExpression(keySelector.Body, keySelector.Parameters[0]);
        }
        #endregion

        #region ToBindingList
#if !SILVERLIGHT
        // Silverlight does not provide an IBindingList interface, so this code would 
        // not compile.

        /// <summary>
        /// Converts a Bindable LINQ binding list.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="bindableCollection">The bindable collection.</param>
        /// <returns></returns>
        public static IBindingList ToBindingList<TElement>(this IBindableCollection<TElement> bindableCollection) where TElement : class
        {
            return new BindingListAdapter<TElement>(bindableCollection);
        }

#endif
        #endregion

        #region Union (DONE)
        /// <summary>Produces the set union of two sequences by using the default equality comparer.</summary>
        /// <returns>An <see cref="T:IBindableCollection`1" /> that contains the elements from both input sequences, excluding duplicates.</returns>
        /// <param name="first">An <see cref="T:IBindableCollection`1" /> whose distinct elements form the first set for the union.</param>
        /// <param name="second">An <see cref="T:IBindableCollection`1" /> whose distinct elements form the second set for the union.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="first" /> or <paramref name="second" /> is null.</exception>
        public static IBindableQuery<TSource> Union<TSource>(this IBindableCollection<TSource> first, IBindableCollection<TSource> second) where TSource : class
        {
            first.ShouldNotBeNull("first");
            second.ShouldNotBeNull("second");
            var sources = new BindableCollection<IBindableCollection<TSource>>();
            sources.AddRange(first, second);
            return new UnionIterator<TSource>(sources);
        }

        /// <summary>Produces the set union of two sequences by using a specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
        /// <returns>An <see cref="T:IBindableCollection`1" /> that contains the elements from both input sequences, excluding duplicates.</returns>
        /// <param name="first">An <see cref="T:IBindableCollection`1" /> whose distinct elements form the first set for the union.</param>
        /// <param name="second">An <see cref="T:IBindableCollection`1" /> whose distinct elements form the second set for the union.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare values.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="first" /> or <paramref name="second" /> is null.</exception>
        public static IBindableQuery<TSource> Union<TSource>(this IBindableCollection<TSource> first, IBindableCollection<TSource> second, IEqualityComparer<TSource> comparer) where TSource : class
        {
            return first.Union(second).Distinct(comparer);
        }
        #endregion

        #region UnionAll (DONE)
        /// <summary>Produces the set union of multiple sequences.</summary>
        /// <returns>An <see cref="T:IBindableCollection`1" /> that contains the elements from both input sequences, excluding duplicates.</returns>
        /// <param name="sources">An <see cref="T:IBindableCollection`1" /> whose elements are also <see cref="T:IBindableCollection`1" /> of the elements to be combined.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="sources" /> is null.</exception>
        public static IBindableQuery<TSource> UnionAll<TSource>(this IBindableCollection<IBindableCollection<TSource>> sources) where TSource : class
        {
            sources.ShouldNotBeNull("sources");
            return new UnionIterator<TSource>(sources);
        }
        #endregion

        #region Where (DONE)
        /// <summary>Filters a sequence of values based on a predicate.</summary>
        /// <returns>An <see cref="T:IBindableCollection`1" /> that contains elements from the input sequence that satisfy the condition.</returns>
        /// <param name="source">An <see cref="T:IBindableCollection`1" /> to filter.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
        public static IBindableQuery<TSource> Where<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, bool>> predicate) where TSource : class
        {
            source.ShouldNotBeNull("source");
            predicate.ShouldNotBeNull("predicate");
            return new WhereIterator<TSource>(source, predicate.Compile()).WithDependencyExpression(predicate.Body, predicate.Parameters[0]);
        }
        #endregion

        #endregion

        #region Aggregators

        #region Aggregate (DONE)
        /// <summary>
        /// Applies an accumulator function over a sequence.
        /// </summary>
        /// <param name="source">An <see cref="IBindableCollection{TSource}" /> to aggregate over.</param>
        /// <param name="func">An accumulator function to be invoked on each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The final accumulator value.</returns>
        public static IBindable<TSource> Aggregate<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, TSource, TSource>> func)
        {
            source.ShouldNotBeNull("source");
            func.ShouldNotBeNull("func");
            return new CustomAggregator<TSource, TSource>(source, default(TSource), func.Compile());
        }

        /// <summary>
        /// Applies an accumulator function over a sequence. The specified seed value is used as the 
        /// initial accumulator value.
        /// </summary>
        /// <param name="source">An <see cref="T:IBindableCollection`1" /> to aggregate over.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <returns>The final accumulator value.</returns>
        public static IBindable<TAccumulate> Aggregate<TSource, TAccumulate>(this IBindableCollection<TSource> source, TAccumulate seed, Expression<Func<TAccumulate, TSource, TAccumulate>> func)
        {
            source.ShouldNotBeNull("source");
            func.ShouldNotBeNull("func");
            seed.ShouldNotBeNull("seed");
            return new CustomAggregator<TSource, TAccumulate>(source, seed, func.Compile());
        }

        /// <summary>
        /// Applies an accumulator function over a sequence. The specified seed value is used as the initial accumulator value, and the specified function is used to select the result value.
        /// </summary>
        /// <param name="source">An <see cref="T:IBindableCollection`1" /> to aggregate over.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element.</param>
        /// <param name="resultSelector">A function to transform the final accumulator value into the result value.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <returns>The transformed final accumulator value.</returns>
        public static IBindable<TResult> Aggregate<TSource, TAccumulate, TResult>(this IBindableCollection<TSource> source, TAccumulate seed, Expression<Func<TAccumulate, TSource, TAccumulate>> func, Expression<Func<TAccumulate, TResult>> resultSelector)
        {
            source.ShouldNotBeNull("source");
            resultSelector.ShouldNotBeNull("resultSelector");
            return Aggregate(source, seed, func).Project(resultSelector);
        }
        #endregion

        #region All (DONE)
        /// <summary>
        /// Determines whether all elements of a sequence satisfy a condition.
        /// </summary>
        /// <param name="source">An <see cref="T:IBindableCollection`1" /> that contains the elements to apply the predicate to.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>
        /// true if every element of the source sequence passes the test in the specified 
        /// predicate, or if the sequence is empty; otherwise, false.
        /// </returns>
        public static IBindable<bool> All<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, bool>> predicate) where TSource : class
        {
            source.ShouldNotBeNull("source");
            predicate.ShouldNotBeNull("predicate");
            return source.Where(predicate).Count().If(count => count >= 1);
        }
        #endregion

        #region Any (DONE)
        /// <summary>
        /// Determines whether a sequence contains any elements.
        /// </summary>
        /// <param name="source">The <see cref="T:IBindableCollection`1" /> to check for emptiness.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>true if the source sequence contains any elements; otherwise, false.</returns>
        public static IBindable<bool> Any<TSource>(this IBindableCollection<TSource> source)
        {
            source.ShouldNotBeNull("source");
            return source.Count().If(count => count >= 1);
        }

        /// <summary>
        /// Determines whether any element of a sequence satisfies a condition.
        /// </summary>
        /// <param name="source">An <see cref="T:IBindableCollection`1" /> whose elements to apply the predicate to.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>true if any elements in the source sequence pass the test in the specified predicate; otherwise, false.</returns>
        public static IBindable<bool> Any<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, bool>> predicate) where TSource : class
        {
            source.ShouldNotBeNull("source");
            predicate.ShouldNotBeNull("predicate");
            return source.Where(predicate).Any();
        }
        #endregion

        #region Average (DONE)
        /// <summary>
        /// Computes the average of a sequence of <see cref="T:System.Decimal" /> values.
        /// </summary>
        /// <param name="source">A sequence of <see cref="T:System.Decimal" /> values to calculate the average of.</param>
        /// <returns>The average of the sequence of values.</returns>
        public static IBindable<decimal> Average(this IBindableCollection<decimal> source)
        {
            source.ShouldNotBeNull("source");
            return new AverageAggregator<decimal, decimal>(source, new DecimalNumeric());
        }

        /// <summary>
        /// Computes the average of a sequence of nullable <see cref="T:System.Decimal" /> values.
        /// </summary>
        /// <param name="source">A sequence of nullable <see cref="T:System.Decimal" /> values to calculate the average of.</param>
        /// <returns>The average of the sequence of values, or null if the source sequence is empty or contains only values that are null.</returns>
        public static IBindable<decimal?> Average(this IBindableCollection<decimal?> source)
        {
            source.ShouldNotBeNull("source");
            return new AverageAggregator<decimal?, decimal?>(source, new DecimalNumeric());
        }

        /// <summary>
        /// Computes the average of a sequence of nullable <see cref="T:System.Double" /> values.
        /// </summary>
        /// <param name="source">A sequence of nullable <see cref="T:System.Double" /> values to calculate the average of.</param>
        /// <returns>The average of the sequence of values, or null if the source sequence is empty or contains only values that are null.</returns>
        public static IBindable<double?> Average(this IBindableCollection<double?> source)
        {
            source.ShouldNotBeNull("source");
            return new AverageAggregator<double?, double?>(source, new DoubleNumeric());
        }

        /// <summary>
        /// Computes the average of a sequence of nullable <see cref="T:System.Int32" /> values.
        /// </summary>
        /// <param name="source">A sequence of nullable <see cref="T:System.Int32" />values to calculate the average of.</param>
        /// <returns>The average of the sequence of values, or null if the source sequence is empty or contains only values that are null.</returns>
        public static IBindable<double?> Average(this IBindableCollection<int?> source)
        {
            source.ShouldNotBeNull("source");
            return new AverageAggregator<int?, double?>(source, new Int32Numeric());
        }

        /// <summary>
        /// Computes the average of a sequence of nullable <see cref="T:System.Int64" /> values.
        /// </summary>
        /// <param name="source">A sequence of nullable <see cref="T:System.Int64" /> values to calculate the average of.</param>
        /// <returns>The average of the sequence of values, or null if the source sequence is empty or contains only values that are null.</returns>
        public static IBindable<double?> Average(this IBindableCollection<long?> source)
        {
            source.ShouldNotBeNull("source");
            return new AverageAggregator<long?, double?>(source, new Int64Numeric());
        }

        /// <summary>
        /// Computes the average of a sequence of nullable <see cref="T:System.Single" /> values.
        /// </summary>
        /// <param name="source">A sequence of nullable <see cref="T:System.Single" /> values to calculate the average of.</param>
        /// <returns>
        /// The average of the sequence of values, or null if the source sequence is empty or contains 
        /// only values that are null.
        /// </returns>
        public static IBindable<float?> Average(this IBindableCollection<float?> source)
        {
            source.ShouldNotBeNull("source");
            return new AverageAggregator<float?, float?>(source, new FloatNumeric());
        }

        /// <summary>
        /// Computes the average of a sequence of <see cref="T:System.Double" /> values.
        /// </summary>
        /// <param name="source">A sequence of <see cref="T:System.Double" /> values to calculate the average of.</param>
        /// <returns>
        /// The average of the sequence of values.
        /// </returns>
        public static IBindable<double> Average(this IBindableCollection<double> source)
        {
            source.ShouldNotBeNull("source");
            return new AverageAggregator<double, double>(source, new DoubleNumeric());
        }

        /// <summary>
        /// Computes the average of a sequence of <see cref="T:System.Int32" /> values.
        /// </summary>
        /// <param name="source">A sequence of <see cref="T:System.Int32" /> values to calculate the average of.</param>
        /// <returns>The average of the sequence of values.</returns>
        public static IBindable<double> Average(this IBindableCollection<int> source)
        {
            source.ShouldNotBeNull("source");
            return new AverageAggregator<int, double>(source, new Int32Numeric());
        }

        /// <summary>
        /// Computes the average of a sequence of <see cref="T:System.Int64" /> values.
        /// </summary>
        /// <param name="source">A sequence of <see cref="T:System.Int64" /> values to calculate the average of.</param>
        /// <returns>The average of the sequence of values.</returns>
        public static IBindable<double> Average(this IBindableCollection<long> source)
        {
            source.ShouldNotBeNull("source");
            return new AverageAggregator<long, double>(source, new Int64Numeric());
        }

        /// <summary>
        /// Computes the average of a sequence of <see cref="T:System.Single" /> values.
        /// </summary>
        /// <param name="source">A sequence of <see cref="T:System.Single" /> values to calculate the average of.</param>
        /// <returns>The average of the sequence of values.</returns>
        public static IBindable<float> Average(this IBindableCollection<float> source)
        {
            source.ShouldNotBeNull("source");
            return new AverageAggregator<float, float>(source, new FloatNumeric());
        }

        /// <summary>
        /// Computes the average of a sequence of <see cref="T:System.Decimal" /> values that are 
        /// obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="source">A sequence of values that are used to calculate an average.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The average of the sequence of values.</returns>
        public static IBindable<decimal> Average<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, decimal>> selector) where TSource : class
        {
            return source.Select(selector).Average();
        }

        /// <summary>
        /// Computes the average of a sequence of <see cref="T:System.Double" /> values that are obtained 
        /// by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The average of the sequence of values.</returns>
        public static IBindable<double> Average<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, double>> selector) where TSource : class
        {
            return source.Select(selector).Average();
        }

        /// <summary>
        /// Computes the average of a sequence of <see cref="T:System.Int32" /> values that are 
        /// obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The average of the sequence of values.</returns>
        public static IBindable<double> Average<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, int>> selector) where TSource : class
        {
            return source.Select(selector).Average();
        }

        /// <summary>
        /// Computes the average of a sequence of <see cref="T:System.Int64" /> values that are obtained 
        /// by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>The average of the sequence of values.</returns>
        public static IBindable<double> Average<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, long>> selector) where TSource : class
        {
            return source.Select(selector).Average();
        }

        /// <summary>
        /// Computes the average of a sequence of nullable <see cref="T:System.Decimal" /> values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The average of the sequence of values, or null if the source sequence is empty or contains only values that are null.</returns>
        public static IBindable<decimal?> Average<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, decimal?>> selector) where TSource : class
        {
            return source.Select(selector).Average();
        }

        /// <summary>
        /// Computes the average of a sequence of nullable <see cref="T:System.Double" /> values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The average of the sequence of values, or null if the source sequence is empty or contains only values that are null.</returns>
        public static IBindable<double?> Average<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, double?>> selector) where TSource : class
        {
            return source.Select(selector).Average();
        }

        /// <summary>
        /// Computes the average of a sequence of nullable <see cref="T:System.Int32" /> values that are obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The average of the sequence of values, or null if the source sequence is empty or contains only values that are null.</returns>
        public static IBindable<double?> Average<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, int?>> selector) where TSource : class
        {
            return source.Select(selector).Average();
        }

        /// <summary>
        /// Computes the average of a sequence of nullable <see cref="T:System.Int64" /> values that are 
        /// obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <returns>The average of the sequence of values, or null if the source sequence is empty or contains only values that are null.</returns>
        public static IBindable<double?> Average<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, long?>> selector) where TSource : class
        {
            return source.Select(selector).Average();
        }

        /// <summary>Computes the average of a sequence of nullable <see cref="T:System.Single" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
        /// <returns>The average of the sequence of values, or null if the source sequence is empty or contains only values that are null.</returns>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IBindable<float?> Average<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, float?>> selector) where TSource : class
        {
            return source.Select(selector).Average();
        }

        /// <summary>Computes the average of a sequence of <see cref="T:System.Single" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
        /// <returns>The average of the sequence of values.</returns>
        /// <param name="source">A sequence of values to calculate the average of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static IBindable<float> Average<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, float>> selector) where TSource : class
        {
            return source.Select(selector).Average();
        }
        #endregion

        #region Contains (DONE)
        /// <summary>Determines whether a sequence contains a specified element by using the default equality comparer.</summary>
        /// <returns>true if the source sequence contains an element that has the specified value; otherwise, false.</returns>
        /// <param name="source">A sequence in which to locate a value.</param>
        /// <param name="value">The value to locate in the sequence.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindable<bool> Contains<TSource>(this IBindableCollection<TSource> source, TSource value) where TSource : class
        {
            return source.Contains(value, null);
        }

        /// <summary>Determines whether a sequence contains a specified element by using a specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
        /// <returns>true if the source sequence contains an element that has the specified value; otherwise, false.</returns>
        /// <param name="source">A sequence in which to locate a value.</param>
        /// <param name="value">The value to locate in the sequence.</param>
        /// <param name="comparer">An equality comparer to compare values.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindable<bool> Contains<TSource>(this IBindableCollection<TSource> source, TSource value, IEqualityComparer<TSource> comparer) where TSource : class
        {
            if (comparer == null)
            {
                comparer = new DefaultComparer<TSource>();
            }
            value.ShouldNotBeNull("value");
            return source.Where(s => comparer.Equals(s, value)).Any();
        }
        #endregion

        #region Count (DONE)
        /// <summary>Returns the number of elements in a sequence.</summary>
        /// <returns>The number of elements in the input sequence.</returns>
        /// <param name="source">A sequence that contains elements to be counted.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        /// <exception cref="T:System.OverflowException">The number of elements in <paramref name="source" /> is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
        public static IBindable<int> Count<TSource>(this IBindableCollection<TSource> source)
        {
            source.ShouldNotBeNull("source");
            return new CountAggregator<TSource>(source);
        }

        /// <summary>Returns a number that represents how many elements in the specified sequence satisfy a condition.</summary>
        /// <returns>A number that represents how many elements in the sequence satisfy the condition in the predicate function.</returns>
        /// <param name="source">A sequence that contains elements to be tested and counted.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
        /// <exception cref="T:System.OverflowException">The number of elements in <paramref name="source" /> is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
        public static IBindable<int> Count<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, bool>> predicate) where TSource : class
        {
            predicate.ShouldNotBeNull("predicate");
            return source.Where(predicate).Count();
        }
        #endregion

        #region ElementAtOrDefault (DONE)
        /// <summary>Returns the element at a specified index in a sequence or a default value if the index is out of range.</summary>
        /// <returns>default(<paramref name="TSource" />) if the index is outside the bounds of the source sequence; otherwise, the element at the specified position in the source sequence.</returns>
        /// <param name="source">An <see cref="T:IBindableCollection`1" /> to return an element from.</param>
        /// <param name="index">The zero-based index of the element to retrieve.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindable<TSource> ElementAtOrDefault<TSource>(this IBindableCollection<TSource> source, int index)
        {
            source.ShouldNotBeNull("source");
            return new ElementAtAggregator<TSource>(source, index);
        }
        #endregion

        #region FirstOrDefault (DONE)
        /// <summary>Returns the first element of a sequence, or a default value if the sequence contains no elements.</summary>
        /// <returns>default(<paramref name="TSource" />) if <paramref name="source" /> is empty; otherwise, the first element in <paramref name="source" />.</returns>
        /// <param name="source">The <see cref="T:IBindableCollection`1" /> to return the first element of.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindable<TSource> First<TSource>(this IBindableCollection<TSource> source)
        {
            return source.FirstOrDefault();
        }

        /// <summary>Returns the first element of the sequence that satisfies a condition or a default value if no such element is found.</summary>
        /// <returns>default(<paramref name="TSource" />) if <paramref name="source" /> is empty or if no element passes the test specified by <paramref name="predicate" />; otherwise, the first element in <paramref name="source" /> that passes the test specified by <paramref name="predicate" />.</returns>
        /// <param name="source">An <see cref="T:IBindableCollection`1" /> to return an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
        public static IBindable<TSource> First<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, bool>> predicate) where TSource : class
        {
            return source.FirstOrDefault(predicate);
        }

        /// <summary>Returns the first element of a sequence, or a default value if the sequence contains no elements.</summary>
        /// <returns>default(<paramref name="TSource" />) if <paramref name="source" /> is empty; otherwise, the first element in <paramref name="source" />.</returns>
        /// <param name="source">The <see cref="T:IBindableCollection`1" /> to return the first element of.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindable<TSource> FirstOrDefault<TSource>(this IBindableCollection<TSource> source)
        {
            source.ShouldNotBeNull("source");
            return source.ElementAtOrDefault(0);
        }

        /// <summary>Returns the first element of the sequence that satisfies a condition or a default value if no such element is found.</summary>
        /// <returns>default(<paramref name="TSource" />) if <paramref name="source" /> is empty or if no element passes the test specified by <paramref name="predicate" />; otherwise, the first element in <paramref name="source" /> that passes the test specified by <paramref name="predicate" />.</returns>
        /// <param name="source">An <see cref="T:IBindableCollection`1" /> to return an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
        public static IBindable<TSource> FirstOrDefault<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, bool>> predicate) where TSource : class
        {
            return source.Where(predicate).FirstOrDefault();
        }
        #endregion

        #region LastOrDefault (DONE)
        /// <summary>Returns the last element of a sequence, or a default value if the sequence contains no elements.</summary>
        /// <returns>default(<paramref name="TSource" />) if the source sequence is empty; otherwise, the last element in the <see cref="T:IBindableCollection`1" />.</returns>
        /// <param name="source">An <see cref="T:IBindableCollection`1" /> to return the last element of.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindable<TSource> LastOrDefault<TSource>(this IBindableCollection<TSource> source)
        {
            return source.ElementAtOrDefault(-1);
        }

        /// <summary>Returns the last element of a sequence that satisfies a condition or a default value if no such element is found.</summary>
        /// <returns>default(<paramref name="TSource" />) if the sequence is empty or if no elements pass the test in the predicate function; otherwise, the last element that passes the test in the predicate function.</returns>
        /// <param name="source">An <see cref="T:IBindableCollection`1" /> to return an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
        public static IBindable<TSource> LastOrDefault<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, bool>> predicate) where TSource : class
        {
            return source.Where(predicate).LastOrDefault();
        }
        #endregion

        #region Max (DONE)
        /// <summary>Returns the maximum value in a sequence of <see cref="T:System.Decimal" /> values.</summary>
        /// <returns>The maximum value in the sequence.</returns>
        /// <param name="source">A sequence of <see cref="T:System.Decimal" /> values to determine the maximum value of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static IBindable<decimal> Max(this IBindableCollection<decimal> source)
        {
            source.ShouldNotBeNull("source");
            return new MaxAggregator<decimal, decimal>(source, new DecimalNumeric());
        }

        /// <summary>Returns the maximum value in a sequence of nullable <see cref="T:System.Double" /> values.</summary>
        /// <returns>A value of type Nullable&lt;Double&gt; in C# or Nullable(Of Double) in Visual Basic that corresponds to the maximum value in the sequence.</returns>
        /// <param name="source">A sequence of nullable <see cref="T:System.Double" /> values to determine the maximum value of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindable<double?> Max(this IBindableCollection<double?> source)
        {
            source.ShouldNotBeNull("source");
            return new MaxAggregator<double?, double?>(source, new DoubleNumeric());
        }

        /// <summary>Returns the maximum value in a sequence of <see cref="T:System.Double" /> values.</summary>
        /// <returns>The maximum value in the sequence.</returns>
        /// <param name="source">A sequence of <see cref="T:System.Double" /> values to determine the maximum value of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static IBindable<double> Max(this IBindableCollection<double> source)
        {
            source.ShouldNotBeNull("source");
            return new MaxAggregator<double, double>(source, new DoubleNumeric());
        }

        /// <summary>Returns the maximum value in a sequence of nullable <see cref="T:System.Int32" /> values.</summary>
        /// <returns>A value of type Nullable&lt;Int32&gt; in C# or Nullable(Of Int32) in Visual Basic that corresponds to the maximum value in the sequence. </returns>
        /// <param name="source">A sequence of nullable <see cref="T:System.Int32" /> values to determine the maximum value of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindable<int?> Max(this IBindableCollection<int?> source)
        {
            source.ShouldNotBeNull("source");
            return new MaxAggregator<int?, double?>(source, new Int32Numeric());
        }

        /// <summary>Returns the maximum value in a sequence of nullable <see cref="T:System.Int64" /> values.</summary>
        /// <returns>A value of type Nullable&lt;Int64&gt; in C# or Nullable(Of Int64) in Visual Basic that corresponds to the maximum value in the sequence. </returns>
        /// <param name="source">A sequence of nullable <see cref="T:System.Int64" /> values to determine the maximum value of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindable<long?> Max(this IBindableCollection<long?> source)
        {
            source.ShouldNotBeNull("source");
            return new MaxAggregator<long?, double?>(source, new Int64Numeric());
        }

        /// <summary>Returns the maximum value in a sequence of <see cref="T:System.Int32" /> values.</summary>
        /// <returns>The maximum value in the sequence.</returns>
        /// <param name="source">A sequence of <see cref="T:System.Int32" /> values to determine the maximum value of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static IBindable<int> Max(this IBindableCollection<int> source)
        {
            source.ShouldNotBeNull("source");
            return new MaxAggregator<int, double>(source, new Int32Numeric());
        }

        /// <summary>Returns the maximum value in a sequence of nullable <see cref="T:System.Decimal" /> values.</summary>
        /// <returns>A value of type Nullable&lt;Decimal&gt; in C# or Nullable(Of Decimal) in Visual Basic that corresponds to the maximum value in the sequence. </returns>
        /// <param name="source">A sequence of nullable <see cref="T:System.Decimal" /> values to determine the maximum value of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindable<decimal?> Max(this IBindableCollection<decimal?> source)
        {
            source.ShouldNotBeNull("source");
            return new MaxAggregator<decimal?, decimal?>(source, new DecimalNumeric());
        }

        /// <summary>Returns the maximum value in a generic sequence.</summary>
        /// <returns>The maximum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindable<TSource> Max<TSource>(this IBindableCollection<TSource> source)
        {
            throw new NotImplementedException();
        }

        /// <summary>Returns the maximum value in a sequence of <see cref="T:System.Int64" /> values.</summary>
        /// <returns>The maximum value in the sequence.</returns>
        /// <param name="source">A sequence of <see cref="T:System.Int64" /> values to determine the maximum value of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static IBindable<long> Max(this IBindableCollection<long> source)
        {
            source.ShouldNotBeNull("source");
            return new MaxAggregator<long, double>(source, new Int64Numeric());
        }

        /// <summary>Returns the maximum value in a sequence of nullable <see cref="T:System.Single" /> values.</summary>
        /// <returns>A value of type Nullable&lt;Single&gt; in C# or Nullable(Of Single) in Visual Basic that corresponds to the maximum value in the sequence.</returns>
        /// <param name="source">A sequence of nullable <see cref="T:System.Single" /> values to determine the maximum value of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindable<float?> Max(this IBindableCollection<float?> source)
        {
            source.ShouldNotBeNull("source");
            return new MaxAggregator<float?, float?>(source, new FloatNumeric());
        }

        /// <summary>Returns the maximum value in a sequence of <see cref="T:System.Single" /> values.</summary>
        /// <returns>The maximum value in the sequence.</returns>
        /// <param name="source">A sequence of <see cref="T:System.Single" /> values to determine the maximum value of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static IBindable<float> Max(this IBindableCollection<float> source)
        {
            source.ShouldNotBeNull("source");
            return new MaxAggregator<float, float>(source, new FloatNumeric());
        }

        /// <summary>Invokes a transform function on each element of a sequence and returns the maximum <see cref="T:System.Decimal" /> value.</summary>
        /// <returns>The maximum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static IBindable<decimal> Max<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, decimal>> selector) where TSource : class
        {
            source.ShouldNotBeNull("source");
            return source.Select(selector).Max();
        }

        /// <summary>Invokes a transform function on each element of a sequence and returns the maximum <see cref="T:System.Double" /> value.</summary>
        /// <returns>The maximum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static IBindable<double> Max<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, double>> selector) where TSource : class
        {
            source.ShouldNotBeNull("source");
            return source.Select(selector).Max();
        }

        /// <summary>Invokes a transform function on each element of a sequence and returns the maximum nullable <see cref="T:System.Decimal" /> value.</summary>
        /// <returns>The value of type Nullable&lt;Decimal&gt; in C# or Nullable(Of Decimal) in Visual Basic that corresponds to the maximum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IBindable<decimal?> Max<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, decimal?>> selector) where TSource : class
        {
            source.ShouldNotBeNull("source");
            return source.Select(selector).Max();
        }

        /// <summary>Invokes a transform function on each element of a sequence and returns the maximum nullable <see cref="T:System.Double" /> value.</summary>
        /// <returns>The value of type Nullable&lt;Double&gt; in C# or Nullable(Of Double) in Visual Basic that corresponds to the maximum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IBindable<double?> Max<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, double?>> selector) where TSource : class
        {
            source.ShouldNotBeNull("source");
            return source.Select(selector).Max();
        }

        /// <summary>Invokes a transform function on each element of a sequence and returns the maximum <see cref="T:System.Int32" /> value.</summary>
        /// <returns>The maximum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static IBindable<int> Max<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, int>> selector) where TSource : class
        {
            source.ShouldNotBeNull("source");
            return source.Select(selector).Max();
        }

        /// <summary>Invokes a transform function on each element of a generic sequence and returns the maximum resulting value.</summary>
        /// <returns>The maximum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by <paramref name="selector" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IBindable<TResult> Max<TSource, TResult>(this IBindableCollection<TSource> source, Expression<Func<TSource, TResult>> selector) where TSource : class
        {
            source.ShouldNotBeNull("source");
            return source.Select(selector).Max();
        }

        /// <summary>Invokes a transform function on each element of a sequence and returns the maximum nullable <see cref="T:System.Int32" /> value.</summary>
        /// <returns>The value of type Nullable&lt;Int32&gt; in C# or Nullable(Of Int32) in Visual Basic that corresponds to the maximum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IBindable<int?> Max<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, int?>> selector) where TSource : class
        {
            source.ShouldNotBeNull("source");
            return source.Select(selector).Max();
        }

        /// <summary>Invokes a transform function on each element of a sequence and returns the maximum nullable <see cref="T:System.Int64" /> value.</summary>
        /// <returns>The value of type Nullable&lt;Int64&gt; in C# or Nullable(Of Int64) in Visual Basic that corresponds to the maximum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IBindable<long?> Max<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, long?>> selector) where TSource : class
        {
            source.ShouldNotBeNull("source");
            return source.Select(selector).Max();
        }

        /// <summary>Invokes a transform function on each element of a sequence and returns the maximum <see cref="T:System.Int64" /> value.</summary>
        /// <returns>The maximum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static IBindable<long> Max<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, long>> selector) where TSource : class
        {
            source.ShouldNotBeNull("source");
            return source.Select(selector).Max();
        }

        /// <summary>Invokes a transform function on each element of a sequence and returns the maximum nullable <see cref="T:System.Single" /> value.</summary>
        /// <returns>The value of type Nullable&lt;Single&gt; in C# or Nullable(Of Single) in Visual Basic that corresponds to the maximum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IBindable<float?> Max<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, float?>> selector) where TSource : class
        {
            source.ShouldNotBeNull("source");
            return source.Select(selector).Max();
        }

        /// <summary>Invokes a transform function on each element of a sequence and returns the maximum <see cref="T:System.Single" /> value.</summary>
        /// <returns>The maximum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static IBindable<float> Max<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, float>> selector) where TSource : class
        {
            source.ShouldNotBeNull("source");
            return source.Select(selector).Max();
        }
        #endregion

        #region Min (DONE)
        /// <summary>Returns the minimum value in a sequence of <see cref="T:System.Decimal" /> values.</summary>
        /// <returns>The minimum value in the sequence.</returns>
        /// <param name="source">A sequence of <see cref="T:System.Decimal" /> values to determine the minimum value of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static IBindable<decimal> Min(this IBindableCollection<decimal> source)
        {
            source.ShouldNotBeNull("source");
            return new MinAggregator<decimal, decimal>(source, new DecimalNumeric());
        }

        /// <summary>Returns the minimum value in a sequence of <see cref="T:System.Double" /> values.</summary>
        /// <returns>The minimum value in the sequence.</returns>
        /// <param name="source">A sequence of <see cref="T:System.Double" /> values to determine the minimum value of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static IBindable<double> Min(this IBindableCollection<double> source)
        {
            source.ShouldNotBeNull("source");
            return new MinAggregator<double, double>(source, new DoubleNumeric());
        }

        /// <summary>Returns the minimum value in a sequence of nullable <see cref="T:System.Decimal" /> values.</summary>
        /// <returns>A value of type Nullable&lt;Decimal&gt; in C# or Nullable(Of Decimal) in Visual Basic that corresponds to the minimum value in the sequence.</returns>
        /// <param name="source">A sequence of nullable <see cref="T:System.Decimal" /> values to determine the minimum value of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindable<decimal?> Min(this IBindableCollection<decimal?> source)
        {
            source.ShouldNotBeNull("source");
            return new MinAggregator<decimal?, decimal?>(source, new DecimalNumeric());
        }

        /// <summary>Returns the minimum value in a sequence of nullable <see cref="T:System.Double" /> values.</summary>
        /// <returns>A value of type Nullable&lt;Double&gt; in C# or Nullable(Of Double) in Visual Basic that corresponds to the minimum value in the sequence.</returns>
        /// <param name="source">A sequence of nullable <see cref="T:System.Double" /> values to determine the minimum value of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindable<double?> Min(this IBindableCollection<double?> source)
        {
            source.ShouldNotBeNull("source");
            return new MinAggregator<double?, double?>(source, new DoubleNumeric());
        }

        /// <summary>Returns the minimum value in a generic sequence.</summary>
        /// <returns>The minimum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindable<TSource> Min<TSource>(this IBindableCollection<TSource> source)
        {
            throw new NotImplementedException();
        }

        /// <summary>Returns the minimum value in a sequence of nullable <see cref="T:System.Int32" /> values.</summary>
        /// <returns>A value of type Nullable&lt;Int32&gt; in C# or Nullable(Of Int32) in Visual Basic that corresponds to the minimum value in the sequence.</returns>
        /// <param name="source">A sequence of nullable <see cref="T:System.Int32" /> values to determine the minimum value of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindable<int?> Min(this IBindableCollection<int?> source)
        {
            source.ShouldNotBeNull("source");
            return new MinAggregator<int?, double?>(source, new Int32Numeric());
        }

        /// <summary>Returns the minimum value in a sequence of nullable <see cref="T:System.Int64" /> values.</summary>
        /// <returns>A value of type Nullable&lt;Int64&gt; in C# or Nullable(Of Int64) in Visual Basic that corresponds to the minimum value in the sequence.</returns>
        /// <param name="source">A sequence of nullable <see cref="T:System.Int64" /> values to determine the minimum value of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindable<long?> Min(this IBindableCollection<long?> source)
        {
            source.ShouldNotBeNull("source");
            return new MinAggregator<long?, double?>(source, new Int64Numeric());
        }

        /// <summary>Returns the minimum value in a sequence of nullable <see cref="T:System.Single" /> values.</summary>
        /// <returns>A value of type Nullable&lt;Single&gt; in C# or Nullable(Of Single) in Visual Basic that corresponds to the minimum value in the sequence.</returns>
        /// <param name="source">A sequence of nullable <see cref="T:System.Single" /> values to determine the minimum value of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindable<float?> Min(this IBindableCollection<float?> source)
        {
            source.ShouldNotBeNull("source");
            return new MinAggregator<float?, float?>(source, new FloatNumeric());
        }

        /// <summary>Returns the minimum value in a sequence of <see cref="T:System.Int32" /> values.</summary>
        /// <returns>The minimum value in the sequence.</returns>
        /// <param name="source">A sequence of <see cref="T:System.Int32" /> values to determine the minimum value of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static IBindable<int> Min(this IBindableCollection<int> source)
        {
            source.ShouldNotBeNull("source");
            return new MinAggregator<int, double>(source, new Int32Numeric());
        }

        /// <summary>Returns the minimum value in a sequence of <see cref="T:System.Int64" /> values.</summary>
        /// <returns>The minimum value in the sequence.</returns>
        /// <param name="source">A sequence of <see cref="T:System.Int64" /> values to determine the minimum value of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static IBindable<long> Min(this IBindableCollection<long> source)
        {
            source.ShouldNotBeNull("source");
            return new MinAggregator<long, double>(source, new Int64Numeric());
        }

        /// <summary>Returns the minimum value in a sequence of <see cref="T:System.Single" /> values.</summary>
        /// <returns>The minimum value in the sequence.</returns>
        /// <param name="source">A sequence of <see cref="T:System.Single" /> values to determine the minimum value of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static IBindable<float> Min(this IBindableCollection<float> source)
        {
            source.ShouldNotBeNull("source");
            return new MinAggregator<float, float>(source, new FloatNumeric());
        }

        /// <summary>Invokes a transform function on each element of a sequence and returns the minimum <see cref="T:System.Decimal" /> value.</summary>
        /// <returns>The minimum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static IBindable<decimal> Min<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, decimal>> selector) where TSource : class
        {
            source.ShouldNotBeNull("source");
            return source.Select(selector).Min();
        }

        /// <summary>Invokes a transform function on each element of a sequence and returns the minimum <see cref="T:System.Double" /> value.</summary>
        /// <returns>The minimum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static IBindable<double> Min<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, double>> selector) where TSource : class
        {
            source.ShouldNotBeNull("source");
            return source.Select(selector).Min();
        }

        /// <summary>Invokes a transform function on each element of a sequence and returns the minimum nullable <see cref="T:System.Int64" /> value.</summary>
        /// <returns>The value of type Nullable&lt;Int64&gt; in C# or Nullable(Of Int64) in Visual Basic that corresponds to the minimum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IBindable<long?> Min<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, long?>> selector) where TSource : class
        {
            source.ShouldNotBeNull("source");
            return source.Select(selector).Min();
        }

        /// <summary>Invokes a transform function on each element of a sequence and returns the minimum <see cref="T:System.Int32" /> value.</summary>
        /// <returns>The minimum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static IBindable<int> Min<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, int>> selector) where TSource : class
        {
            source.ShouldNotBeNull("source");
            return source.Select(selector).Min();
        }

        /// <summary>Invokes a transform function on each element of a sequence and returns the minimum nullable <see cref="T:System.Double" /> value.</summary>
        /// <returns>The value of type Nullable&lt;Double&gt; in C# or Nullable(Of Double) in Visual Basic that corresponds to the minimum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IBindable<double?> Min<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, double?>> selector) where TSource : class
        {
            source.ShouldNotBeNull("source");
            return source.Select(selector).Min();
        }

        /// <summary>Invokes a transform function on each element of a sequence and returns the minimum nullable <see cref="T:System.Single" /> value.</summary>
        /// <returns>The value of type Nullable&lt;Single&gt; in C# or Nullable(Of Single) in Visual Basic that corresponds to the minimum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IBindable<float?> Min<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, float?>> selector) where TSource : class
        {
            source.ShouldNotBeNull("source");
            return source.Select(selector).Min();
        }

        /// <summary>Invokes a transform function on each element of a sequence and returns the minimum <see cref="T:System.Int64" /> value.</summary>
        /// <returns>The minimum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static IBindable<long> Min<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, long>> selector) where TSource : class
        {
            source.ShouldNotBeNull("source");
            return source.Select(selector).Min();
        }

        /// <summary>Invokes a transform function on each element of a sequence and returns the minimum nullable <see cref="T:System.Decimal" /> value.</summary>
        /// <returns>The value of type Nullable&lt;Decimal&gt; in C# or Nullable(Of Decimal) in Visual Basic that corresponds to the minimum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IBindable<decimal?> Min<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, decimal?>> selector) where TSource : class
        {
            source.ShouldNotBeNull("source");
            return source.Select(selector).Min();
        }

        /// <summary>Invokes a transform function on each element of a sequence and returns the minimum nullable <see cref="T:System.Int32" /> value.</summary>
        /// <returns>The value of type Nullable&lt;Int32&gt; in C# or Nullable(Of Int32) in Visual Basic that corresponds to the minimum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IBindable<int?> Min<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, int?>> selector) where TSource : class
        {
            source.ShouldNotBeNull("source");
            return source.Select(selector).Min();
        }

        /// <summary>Invokes a transform function on each element of a sequence and returns the minimum <see cref="T:System.Single" /> value.</summary>
        /// <returns>The minimum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="source" /> contains no elements.</exception>
        public static IBindable<float> Min<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, float>> selector) where TSource : class
        {
            source.ShouldNotBeNull("source");
            return source.Select(selector).Min();
        }

        /// <summary>Invokes a transform function on each element of a generic sequence and returns the minimum resulting value.</summary>
        /// <returns>The minimum value in the sequence.</returns>
        /// <param name="source">A sequence of values to determine the minimum value of.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by <paramref name="selector" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IBindable<TResult> Min<TSource, TResult>(this IBindableCollection<TSource> source, Expression<Func<TSource, TResult>> selector) where TSource : class
        {
            return source.Select(selector).Min();
        }
        #endregion

        #region SequenceEqual (NOT)
        /// <summary>Determines whether two sequences are equal by comparing the elements by using the default equality comparer for their type.</summary>
        /// <returns>true if the two source sequences are of equal length and their corresponding elements are equal according to the default equality comparer for their type; otherwise, false.</returns>
        /// <param name="first">An <see cref="T:IBindableCollection`1" /> to compare to <paramref name="second" />.</param>
        /// <param name="second">An <see cref="T:IBindableCollection`1" /> to compare to the first sequence.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="first" /> or <paramref name="second" /> is null.</exception>
        public static IBindable<bool> SequenceEqual<TSource>(this IBindableCollection<TSource> first, IEnumerable<TSource> second)
        {
            throw new NotImplementedException();
        }

        /// <summary>Determines whether two sequences are equal by comparing their elements by using a specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
        /// <returns>true if the two source sequences are of equal length and their corresponding elements compare equal according to <paramref name="comparer" />; otherwise, false.</returns>
        /// <param name="first">An <see cref="T:IBindableCollection`1" /> to compare to <paramref name="second" />.</param>
        /// <param name="second">An <see cref="T:IBindableCollection`1" /> to compare to the first sequence.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to use to compare elements.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="first" /> or <paramref name="second" /> is null.</exception>
        public static IBindable<bool> SequenceEqual<TSource>(this IBindableCollection<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Single (DONE)
        /// <summary>Returns the only element of a sequence, and throws an exception if there is not exactly one element in the sequence.</summary>
        /// <returns>The single element of the input sequence.</returns>
        /// <param name="source">An <see cref="T:IBindableCollection`1" /> to return the single element of.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The input sequence contains more than one element.-or-The input sequence is empty.</exception>
        public static IBindable<TSource> Single<TSource>(this IBindableCollection<TSource> source)
        {
            return source.FirstOrDefault();
        }

        /// <summary>Returns the only element of a sequence that satisfies a specified condition, and throws an exception if more than one such element exists.</summary>
        /// <returns>The single element of the input sequence that satisfies a condition.</returns>
        /// <param name="source">An <see cref="T:IBindableCollection`1" /> to return a single element from.</param>
        /// <param name="predicate">A function to test an element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">No element satisfies the condition in <paramref name="predicate" />.-or-More than one element satisfies the condition in <paramref name="predicate" />.-or-The source sequence is empty.</exception>
        public static IBindable<TSource> Single<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, bool>> predicate) where TSource : class
        {
            return source.FirstOrDefault(predicate);
        }
        #endregion

        #region SingleOrDefault (DONE)
        /// <summary>Returns the only element of a sequence, or a default value if the sequence is empty; this method throws an exception if there is more than one element in the sequence.</summary>
        /// <returns>The single element of the input sequence, or default(<paramref name="TSource" />) if the sequence contains no elements.</returns>
        /// <param name="source">An <see cref="T:IBindableCollection`1" /> to return the single element of.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The input sequence contains more than one element.</exception>
        public static IBindable<TSource> SingleOrDefault<TSource>(this IBindableCollection<TSource> source)
        {
            return source.FirstOrDefault();
        }

        /// <summary>Returns the only element of a sequence that satisfies a specified condition or a default value if no such element exists; this method throws an exception if more than one element satisfies the condition.</summary>
        /// <returns>The single element of the input sequence that satisfies the condition, or default(<paramref name="TSource" />) if no such element is found.</returns>
        /// <param name="source">An <see cref="T:IBindableCollection`1" /> to return a single element from.</param>
        /// <param name="predicate">A function to test an element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="predicate" /> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">More than one element satisfies the condition in <paramref name="predicate" />.</exception>
        public static IBindable<TSource> SingleOrDefault<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, bool>> predicate) where TSource : class
        {
            return source.FirstOrDefault(predicate);
        }
        #endregion

        #region Sum (DONE)
        /// <summary>Computes the sum of a sequence of <see cref="T:System.Double" /> values.</summary>
        /// <returns>The sum of the values in the sequence.</returns>
        /// <param name="source">A sequence of <see cref="T:System.Double" /> values to calculate the sum of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindable<double> Sum(this IBindableCollection<double> source)
        {
            return new SumAggregator<double, double>(source, new DoubleNumeric());
        }

        /// <summary>Computes the sum of a sequence of nullable <see cref="T:System.Decimal" /> values.</summary>
        /// <returns>The sum of the values in the sequence.</returns>
        /// <param name="source">A sequence of nullable <see cref="T:System.Decimal" /> values to calculate the sum of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
        public static IBindable<decimal?> Sum(this IBindableCollection<decimal?> source)
        {
            return new SumAggregator<decimal?, decimal?>(source, new DecimalNumeric());
        }

        /// <summary>Computes the sum of a sequence of <see cref="T:System.Decimal" /> values.</summary>
        /// <returns>The sum of the values in the sequence.</returns>
        /// <param name="source">A sequence of <see cref="T:System.Decimal" /> values to calculate the sum of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
        public static IBindable<decimal> Sum(this IBindableCollection<decimal> source)
        {
            return new SumAggregator<decimal, decimal>(source, new DecimalNumeric());
        }

        /// <summary>Computes the sum of a sequence of <see cref="T:System.Int32" /> values.</summary>
        /// <returns>The sum of the values in the sequence.</returns>
        /// <param name="source">A sequence of <see cref="T:System.Int32" /> values to calculate the sum of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
        public static IBindable<int> Sum(this IBindableCollection<int> source)
        {
            return new SumAggregator<int, double>(source, new Int32Numeric());
        }

        /// <summary>Computes the sum of a sequence of <see cref="T:System.Int64" /> values.</summary>
        /// <returns>The sum of the values in the sequence.</returns>
        /// <param name="source">A sequence of <see cref="T:System.Int64" /> values to calculate the sum of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int64.MaxValue" />.</exception>
        public static IBindable<long> Sum(this IBindableCollection<long> source)
        {
            return new SumAggregator<long, double>(source, new Int64Numeric());
        }

        /// <summary>Computes the sum of a sequence of nullable <see cref="T:System.Double" /> values.</summary>
        /// <returns>The sum of the values in the sequence.</returns>
        /// <param name="source">A sequence of nullable <see cref="T:System.Double" /> values to calculate the sum of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindable<double?> Sum(this IBindableCollection<double?> source)
        {
            return new SumAggregator<double?, double?>(source, new DoubleNumeric());
        }

        /// <summary>Computes the sum of a sequence of nullable <see cref="T:System.Int32" /> values.</summary>
        /// <returns>The sum of the values in the sequence.</returns>
        /// <param name="source">A sequence of nullable <see cref="T:System.Int32" /> values to calculate the sum of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
        public static IBindable<int?> Sum(this IBindableCollection<int?> source)
        {
            return new SumAggregator<int?, double?>(source, new Int32Numeric());
        }

        /// <summary>Computes the sum of a sequence of nullable <see cref="T:System.Int64" /> values.</summary>
        /// <returns>The sum of the values in the sequence.</returns>
        /// <param name="source">A sequence of nullable <see cref="T:System.Int64" /> values to calculate the sum of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int64.MaxValue" />.</exception>
        public static IBindable<long?> Sum(this IBindableCollection<long?> source)
        {
            return new SumAggregator<long?, double?>(source, new Int64Numeric());
        }

        /// <summary>Computes the sum of a sequence of nullable <see cref="T:System.Single" /> values.</summary>
        /// <returns>The sum of the values in the sequence.</returns>
        /// <param name="source">A sequence of nullable <see cref="T:System.Single" /> values to calculate the sum of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindable<float?> Sum(this IBindableCollection<float?> source)
        {
            return new SumAggregator<float?, float?>(source, new FloatNumeric());
        }

        /// <summary>Computes the sum of a sequence of <see cref="T:System.Single" /> values.</summary>
        /// <returns>The sum of the values in the sequence.</returns>
        /// <param name="source">A sequence of <see cref="T:System.Single" /> values to calculate the sum of.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is null.</exception>
        public static IBindable<float> Sum(this IBindableCollection<float> source)
        {
            return new SumAggregator<float, float>(source, new FloatNumeric());
        }

        /// <summary>Computes the sum of the sequence of nullable <see cref="T:System.Decimal" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
        /// <returns>The sum of the projected values.</returns>
        /// <param name="source">A sequence of values that are used to calculate a sum.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
        public static IBindable<decimal?> Sum<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, decimal?>> selector) where TSource : class
        {
            return source.Select(selector).Sum();
        }

        /// <summary>Computes the sum of the sequence of nullable <see cref="T:System.Double" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
        /// <returns>The sum of the projected values.</returns>
        /// <param name="source">A sequence of values that are used to calculate a sum.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IBindable<double?> Sum<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, double?>> selector) where TSource : class
        {
            return source.Select(selector).Sum();
        }

        /// <summary>Computes the sum of the sequence of <see cref="T:System.Decimal" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
        /// <returns>The sum of the projected values.</returns>
        /// <param name="source">A sequence of values that are used to calculate a sum.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue" />.</exception>
        public static IBindable<decimal> Sum<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, decimal>> selector) where TSource : class
        {
            return source.Select(selector).Sum();
        }

        /// <summary>Computes the sum of the sequence of nullable <see cref="T:System.Int32" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
        /// <returns>The sum of the projected values.</returns>
        /// <param name="source">A sequence of values that are used to calculate a sum.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
        public static IBindable<int?> Sum<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, int?>> selector) where TSource : class
        {
            return source.Select(selector).Sum();
        }

        /// <summary>Computes the sum of the sequence of <see cref="T:System.Double" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
        /// <returns>The sum of the projected values.</returns>
        /// <param name="source">A sequence of values that are used to calculate a sum.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IBindable<double> Sum<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, double>> selector) where TSource : class
        {
            return source.Select(selector).Sum();
        }

        /// <summary>Computes the sum of the sequence of <see cref="T:System.Int32" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
        /// <returns>The sum of the projected values.</returns>
        /// <param name="source">A sequence of values that are used to calculate a sum.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
        public static IBindable<int> Sum<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, int>> selector) where TSource : class
        {
            return source.Select(selector).Sum();
        }

        /// <summary>Computes the sum of the sequence of <see cref="T:System.Int64" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
        /// <returns>The sum of the projected values.</returns>
        /// <param name="source">A sequence of values that are used to calculate a sum.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int64.MaxValue" />.</exception>
        public static IBindable<long> Sum<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, long>> selector) where TSource : class
        {
            return source.Select(selector).Sum();
        }

        /// <summary>Computes the sum of the sequence of nullable <see cref="T:System.Int64" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
        /// <returns>The sum of the projected values.</returns>
        /// <param name="source">A sequence of values that are used to calculate a sum.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        /// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int64.MaxValue" />.</exception>
        public static IBindable<long?> Sum<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, long?>> selector) where TSource : class
        {
            return source.Select(selector).Sum();
        }

        /// <summary>Computes the sum of the sequence of nullable <see cref="T:System.Single" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
        /// <returns>The sum of the projected values.</returns>
        /// <param name="source">A sequence of values that are used to calculate a sum.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IBindable<float?> Sum<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, float?>> selector) where TSource : class
        {
            return source.Select(selector).Sum();
        }

        /// <summary>Computes the sum of the sequence of <see cref="T:System.Single" /> values that are obtained by invoking a transform function on each element of the input sequence.</summary>
        /// <returns>The sum of the projected values.</returns>
        /// <param name="source">A sequence of values that are used to calculate a sum.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> or <paramref name="selector" /> is null.</exception>
        public static IBindable<float> Sum<TSource>(this IBindableCollection<TSource> source, Expression<Func<TSource, float>> selector) where TSource : class
        {
            return source.Select(selector).Sum();
        }
        #endregion

        #endregion

        #region Operators

        #region If (DONE)
        /// <summary>
        /// Checks a condition on a specified source.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="condition">The condition.</param>
        /// <returns></returns>
        public static IBindable<bool> If<TSource>(this IBindable<TSource> source, Expression<Func<TSource, bool>> condition)
        {
            return source.If(condition, true, false, false);
        }

        /// <summary>
        /// Checks a condition on a specified source.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="valueIfTrue">The value to return if true.</param>
        /// <returns></returns>
        public static IBindable<TResult> If<TSource, TResult>(this IBindable<TSource> source, Expression<Func<TSource, bool>> condition, TResult valueIfTrue)
        {
            return source.If(condition, valueIfTrue, default(TResult), default(TResult));
        }

        /// <summary>
        /// Checks a condition on a specified source.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="valueIfTrue">The value to return if true.</param>
        /// <param name="valueIfFalse">The value to return if false.</param>
        /// <returns></returns>
        public static IBindable<TResult> If<TSource, TResult>(this IBindable<TSource> source, Expression<Func<TSource, bool>> condition, TResult valueIfTrue, TResult valueIfFalse)
        {
            return source.If(condition, valueIfTrue, valueIfFalse, default(TResult));
        }

        /// <summary>
        /// Checks a condition on a specified source.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="valueIfTrue">The value to return if true.</param>
        /// <param name="valueIfFalse">The value to return if false.</param>
        /// <param name="valueIfNull">The value to return if the source is null.</param>
        /// <returns></returns>
        public static IBindable<TResult> If<TSource, TResult>(this IBindable<TSource> source, Expression<Func<TSource, bool>> condition, TResult valueIfTrue, TResult valueIfFalse, TResult valueIfNull)
        {
            return source.If(condition, c => valueIfTrue, c => valueIfFalse, () => valueIfNull);
        }

        /// <summary>
        /// Checks a condition on a specified source.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="expressionIfTrue">The expression to evaluate if true.</param>
        /// <returns></returns>
        public static IBindable<TResult> If<TSource, TResult>(this IBindable<TSource> source, Expression<Func<TSource, bool>> condition, Expression<Func<TSource, TResult>> expressionIfTrue)
        {
            return source.If(condition, expressionIfTrue, c => default(TResult), () => default(TResult));
        }

        /// <summary>
        /// Checks a condition on a specified source.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="valueIfTrue">The value to return if true.</param>
        /// <param name="expressionIfFalse">The expression to evaluate if false.</param>
        /// <returns></returns>
        public static IBindable<TResult> If<TSource, TResult>(this IBindable<TSource> source, Expression<Func<TSource, bool>> condition, TResult valueIfTrue, Expression<Func<TSource, TResult>> expressionIfFalse)
        {
            return source.If(condition, c => valueIfTrue, expressionIfFalse, () => default(TResult));
        }

        /// <summary>
        /// Checks a condition on a specified source.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="expressionIfTrue">The expression to evaluate if true.</param>
        /// <param name="valueIfFalse">The value to return if false.</param>
        /// <returns></returns>
        public static IBindable<TResult> If<TSource, TResult>(this IBindable<TSource> source, Expression<Func<TSource, bool>> condition, Expression<Func<TSource, TResult>> expressionIfTrue, TResult valueIfFalse)
        {
            return source.If(condition, expressionIfTrue, c => valueIfFalse, () => default(TResult));
        }

        /// <summary>
        /// Checks a condition on a specified source.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="expressionIfTrue">The expression to evaluate if true.</param>
        /// <param name="expressionIfFalse">The expression to evaluate if false.</param>
        /// <returns></returns>
        public static IBindable<TResult> If<TSource, TResult>(this IBindable<TSource> source, Expression<Func<TSource, bool>> condition, Expression<Func<TSource, TResult>> expressionIfTrue, Expression<Func<TSource, TResult>> expressionIfFalse)
        {
            return source.If(condition, expressionIfTrue, expressionIfFalse, () => default(TResult));
        }

        /// <summary>
        /// Checks a condition on a specified source.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="expressionIfTrue">The expression to evaluate if true.</param>
        /// <param name="expressionIfFalse">The expression to evaluate if false.</param>
        /// <param name="expressionIfNull">The expression to evaluate if the source is null.</param>
        /// <returns></returns>
        public static IBindable<TResult> If<TSource, TResult>(this IBindable<TSource> source, Expression<Func<TSource, bool>> condition, Expression<Func<TSource, TResult>> expressionIfTrue, Expression<Func<TSource, TResult>> expressionIfFalse, Expression<Func<TResult>> expressionIfNull)
        {
            source.ShouldNotBeNull("source");
            condition.ShouldNotBeNull("filter");

            var compiledValueIfTrue = expressionIfTrue != null ? expressionIfTrue.Compile() : null;
            var compiledValueIfFalse = expressionIfFalse != null ? expressionIfFalse.Compile() : null;
            var compiledValueIfNull = expressionIfNull != null ? expressionIfNull.Compile() : null;

            var result = new IfOperator<TSource, TResult>(source, condition.Compile(), compiledValueIfTrue, compiledValueIfFalse, compiledValueIfNull);
            if (expressionIfTrue != null)
            {
                result.WithDependencyExpression(expressionIfTrue.Body, expressionIfTrue.Parameters[0]);
            }
            if (expressionIfFalse != null)
            {
                result.WithDependencyExpression(expressionIfFalse.Body, expressionIfFalse.Parameters[0]);
            }
            if (expressionIfNull != null)
            {
                result.WithDependencyExpression(expressionIfNull.Body, null);
            }
            return result;
        }
        #endregion

        #region Project (DONE)
        /// <summary>
        /// Projects a single bindable object into another bindable object, using a lambda to select the new
        /// type of object.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source colection.</param>
        /// <param name="projector">The projector function used to turn the source type into the result type.</param>
        /// <returns>An object created by the <paramref name="projector"/>. If the source value changes, the item will be projected again.</returns>
        public static IBindable<TResult> Project<TSource, TResult>(this IBindable<TSource> source, Expression<Func<TSource, TResult>> projector)
        {
            source.ShouldNotBeNull("source");
            projector.ShouldNotBeNull("projector");
            return new ProjectOperator<TSource, TResult>(source, projector.Compile()).WithDependencyExpression(projector.Body, projector.Parameters[0]);
        }
        #endregion

        #endregion

        #region Dependencies
        /// <summary>
        /// Extracts dependencies from the given expression and adds them to the iterator.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="itemParameter">The item parameter.</param>
        /// <returns></returns>
        public static TResult WithDependencyExpression<TResult>(this TResult query, System.Linq.Expressions.Expression expression, ParameterExpression itemParameter) where TResult : IAcceptsDependencies, IConfigurable
        {
            var analyzer = query.Configuration.CreateExpressionAnalyzer();
            var definitions = analyzer.DiscoverDependencies(expression, itemParameter);
            return query.WithDependencies(definitions);
        }

        /// <summary>
        /// Adds a dependency for a given property on a given object external to the query.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="externalObject">The external object to monitor for changes. For example, this could be a regular 
        /// class object, an object that implements <see cref="INotifyCollectionChanged" />, or a Windows Forms control.</param>
        /// <param name="propertyPath">The property path. For example: "HomeAddress.Postcode".</param>
        /// <returns></returns>
        public static TResult WithDependency<TResult>(this TResult query, object externalObject, string propertyPath) where TResult : IAcceptsDependencies
        {
            return query.WithDependency(new ExternalDependencyDefinition(propertyPath, externalObject));
        }

#if SILVERLIGHT

    /// <summary>
    /// Adds a dependency on a Silverlight dependency object.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">The query.</param>
    /// <param name="dependencyObject">A Silverlight dependency object.</param>
    /// <param name="propertyPath">The name of a property on the dependency object.</param>
    /// <returns></returns>
        public static TResult WithDependency<TResult>(this TResult query, System.Windows.DependencyObject dependencyObject, string propertyPath)
            where TResult : IAcceptsDependencies
        {
            return query.WithDependency(new ExternalDependencyDefinition(propertyPath, dependencyObject));
        }

#else

        /// <summary>
        /// Adds a dependency on a WPF dependency object.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="dependencyObject">A WPF dependency object.</param>
        /// <param name="dependencyProperty">A WPF dependency property.</param>
        /// <returns></returns>
        public static TResult WithDependency<TResult>(this TResult query, DependencyObject dependencyObject, DependencyProperty dependencyProperty) where TResult : IAcceptsDependencies
        {
            return query.WithDependency(new ExternalDependencyDefinition(dependencyProperty.Name, dependencyObject));
        }

#endif

        /// <summary>
        /// Adds a dependency on items in the source collection, given the path to a property.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="propertyPath">The property name or path. For example: "HomeAddress.Postcode".</param>
        /// <returns></returns>
        public static TResult WithDependency<TResult>(this TResult query, string propertyPath) where TResult : IAcceptsDependencies
        {
            return query.WithDependency(new ItemDependencyDefinition(propertyPath));
        }

        /// <summary>
        /// Adds a dependency to the Bindable LINQ query given a dependency definition. This allows developers to create custom 
        /// dependency types by implementing the <see cref="IDependencyDefinition"/> interface.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="definition">The definition.</param>
        /// <returns></returns>
        public static TResult WithDependency<TResult>(this TResult query, IDependencyDefinition definition) where TResult : IAcceptsDependencies
        {
            if (query != null && definition != null)
            {
                query.AcceptDependency(definition);
            }
            return query;
        }

        /// <summary>
        /// Adds a set of dependencies to the Bindable LINQ query.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="definitions">The definitions.</param>
        /// <returns></returns>
        public static TResult WithDependencies<TResult>(this TResult query, IEnumerable<IDependencyDefinition> definitions) where TResult : IAcceptsDependencies
        {
            if (query != null)
            {
                foreach (var definition in definitions)
                {
                    if (definition != null)
                    {
                        query.AcceptDependency(definition);
                    }
                }
            }
            return query;
        }
        #endregion
    }
}