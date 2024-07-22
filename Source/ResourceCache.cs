using System;

namespace DotNetResourcesExtensions.Collections
{
    /// <summary>
    /// Defines a temporary backing store where resources can be temporarily saved and retrieved for the application lifetime. <br />
    /// It is expected that after application shutdown , this cache must have been deleted.
    /// </summary>
    public interface IResourceCache : IResourceEntryEnumerable , IDisposable
    {
        /// <summary>
        /// Adds a new resource entry to be cached.
        /// </summary>
        /// <param name="entry">The resource entry to be cached.</param>
        public void Add(IResourceEntry entry);

        /// <summary>
        /// Adds a new resource to be cached with the specified <paramref name="Name"/> and <paramref name="Value"/>.
        /// </summary>
        /// <param name="Name">The resource name to add.</param>
        /// <param name="Value">The resource value to add.</param>
        public void Add(System.String Name , System.Object Value);

        /// <summary>
        /// Adds a new resource that has the specified name and value from the <typeparamref name="TA"/> type.
        /// </summary>
        /// <typeparam name="TA">The resource value type that the resource must be saved in.</typeparam>
        /// <param name="Name">The resource name to add.</param>
        /// <param name="Value">The resource value to add.</param>
        public void Add<TA>(System.String Name , TA Value) where TA : notnull;

        /// <summary>
        /// Removes the first occurence of the specified resource entry defined in <paramref name="entry"/>.
        /// </summary>
        /// <param name="entry">The resource entry to remove.</param>
        /// <returns><see langword="true"/> if at least one occurence of the specified resource entry was found and deleted; otherwise , <see langword="false"/>.</returns>
        public System.Boolean Remove(IResourceEntry entry);

        /// <summary>
        /// Removes the first occurence of the specified resource which has the specified name and value.
        /// </summary>
        /// <param name="Name">The resource name to search so as to be deleted.</param>
        /// <param name="Value">The resource value to search so as to be deleted.</param>
        /// <returns><see langword="true"/> if at least one occurence of the specified resource was found and deleted; otherwise , <see langword="false"/>.</returns>
        public System.Boolean Remove(System.String Name , System.Object Value);

        /// <summary>
        /// Removes the first occurence of the specified resource which has the specified name and value. <br />
        /// The implementers must also implement type equality checks on <paramref name="Value"/> parameter using the <typeparamref name="TR"/> type to check the resource value.
        /// </summary>
        /// <typeparam name="TR">The type of the resource value to additionally check for type equality.</typeparam>
        /// <param name="Name">The resource name to search so as to be deleted.</param>
        /// <param name="Value">The resource value to search so as to be deleted.</param>
        /// <returns><see langword="true"/> if at least one occurence of the specified resource was found and deleted; otherwise , <see langword="false"/>.</returns>
        public System.Boolean Remove<TR>(System.String Name , TR Value) where TR : notnull;

        /// <summary>
        /// Defines the number of cached resources that this <see cref="IResourceCache"/> holds.
        /// </summary>
        public System.UInt32 Count { get; }

        /// <summary>
        /// Updates all the cached items inside the cache , if possible. <br />
        /// NOTE: This is an optional method , which it means that you can safely define an empty method for this.
        /// </summary>
        public void Update();

        /// <summary>
        /// Optimizes all the cached items inside the cache , if possible. <br />
        /// Resource cache optimization is a process which the resource items that have not been used for a while 
        /// are deleted. <br />
        /// NOTE: This is an optional method , which it means that you can safely define an empty method for this.
        /// </summary>
        public void Optimize();

        /// <summary>
        /// Clears all the resource entries from the cache , and optionally define whether to update the cache for looking up any leftovers.
        /// </summary>
        /// <param name="update"></param>
        public void Clear(System.Boolean update = false);

        /// <summary>
        /// Gets the resource entry which has as a name the name defined by the <paramref name="name"/> parameter.
        /// </summary>
        /// <param name="name">The resource name to find the resource for.</param>
        /// <returns>The resource entry defined by <paramref name="name"/> if found; otherwise it throws <see cref="System.Collections.Generic.KeyNotFoundException"/>.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">The resource name defined in <paramref name="name"/> was not found.</exception>
        public IResourceEntry this[System.String name] { get; }

        /// <summary>
        /// Gets the resource entry inside the internal pooled array in the specified index. If the index does not exist , it throws <see cref="IndexOutOfRangeException"/>.
        /// </summary>
        /// <param name="index">The index to retrieve the resource entry at the specified position.</param>
        /// <returns>The resource entry at <paramref name="index"/> if found; otherwise it throws <see cref="IndexOutOfRangeException"/>.</returns>
        /// <exception cref="IndexOutOfRangeException">The <paramref name="index"/> parameter defined an invalid index or was outside the pooled array bounds.</exception>
        public IResourceEntry this[System.Int32 index] { get; }
    }
}
