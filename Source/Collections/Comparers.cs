
using System.Collections.Generic;

namespace DotNetResourcesExtensions.Collections
{

    /// <summary>
    /// Compares two resource entries by their name. <br />
    /// Usually the name is enough to infer that two resource entries are equal.
    /// </summary>
    public sealed class ResourceEntryComparer : Comparer<IResourceEntry>
    {
        /// <summary>
        /// Compares two <see cref="IResourceEntry"/> instances.
        /// </summary>
        /// <param name="x">The first entry to compare.</param>
        /// <param name="y">The second entry to compare.</param>
        /// <returns><inheritdoc cref="Comparer{T}.Compare(T, T)"/></returns>
        public override int Compare(IResourceEntry x, IResourceEntry y) => x.Name.CompareTo(y.Name);

        /// <summary>
        /// Creates a new instance of this resource entry comparer.
        /// </summary>
        public ResourceEntryComparer() : base() { }
    }

    /// <summary>
    /// Defines the default and recommended comparer for comparing two <see cref="IResourceEntryWithComment"/> instances.
    /// </summary>
    public class ResourceEntryWithCommentComparer : Comparer<IResourceEntryWithComment>
    {
        /// <summary>
        /// Compares two <see cref="IResourceEntryWithComment"/> instances.
        /// </summary>
        /// <param name="x">The first entry to compare.</param>
        /// <param name="y">The second entry to compare.</param>
        /// <returns><inheritdoc cref="Comparer{T}.Compare(T, T)"/></returns>
        public override int Compare(IResourceEntryWithComment x, IResourceEntryWithComment y) => x.Name.CompareTo(y.Name);

        /// <summary>
        /// Creates a new instance of this resource entry comparer.
        /// </summary>
        public ResourceEntryWithCommentComparer() : base() { }
    }

}
