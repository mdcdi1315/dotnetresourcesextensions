
using System;
using System.Collections.Generic;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// The <see cref="IResourceLoader"/> interface provides a simple implementation for reading arbitrary resources from 
    /// any resources storage. It provides convenient methods that you can use to load your resources.
    /// </summary>
    public interface IResourceLoader : IResourceEnumerable, IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Gets a string resource. Throws <see cref="ResourceNotFoundException"/> if not found.
        /// </summary>
        /// <param name="Name">The resource name to look up.</param>
        /// <returns>The string resource defined by <paramref name="Name"/>.</returns>
        /// <exception cref="ResourceNotFoundException">The specified resource was not found.</exception>
        /// <exception cref="ResourceTypeMismatchException">The resource found was not a string.</exception>
        public System.String GetStringResource(System.String Name);

        /// <summary>
        /// Get all the resource names where the <paramref name="Name"/> provided starts with them , plus itself.
        /// </summary>
        /// <param name="Name">The resource name to look up.</param>
        /// <returns>An enumerable with relative resource names.</returns>
        public IEnumerable<System.String> GetRelativeResources(System.String Name);

        /// <summary>
        /// Gets all resource names which start with the contents of <paramref name="start"/> parameter.
        /// </summary>
        /// <param name="start">The string which will be used as a search filter.</param>
        /// <returns>All the resource names which start with <paramref name="start"/>.</returns>
        public IEnumerable<System.String> GetResourceNamesStartingWith(System.String start);

        /// <summary>
        /// Gets all the resource names that belong to the current instance.
        /// </summary>
        /// <returns>All the resource names defined in this instance.</returns>
        public IEnumerable<System.String> GetAllResourceNames();

        /// <summary>
        /// Gets a resource of the specified type and returns it. 
        /// </summary>
        /// <typeparam name="T">The type of the resource to return.</typeparam>
        /// <param name="Name">The name of the resource to get.</param>
        /// <returns>The specified resource by it's <paramref name="Name"/> , casted to <typeparamref name="T"/>.</returns>
        /// <exception cref="ResourceNotFoundException">The specified resource was not found.</exception>
        /// <exception cref="ResourceTypeMismatchException">The specified resource does not have as a type the type passed in <typeparamref name="T"/>.</exception>
        public T GetResource<T>(System.String Name) where T : notnull;

        /// <summary>
        /// Disposes any allocated data used by the implementation class.
        /// </summary>
        public new void Dispose();

        /// <summary>
        /// Disposes asynchronously any allocated data used by the implementation class.
        /// </summary>
        public new System.Threading.Tasks.ValueTask DisposeAsync();
    }
}
