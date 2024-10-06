
using System;
using System.Collections;
using System.Collections.Generic;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Represents a default implementation of the <see cref="IResourceLoader"/> interface. <br />
    /// This class also provides optimizations for string resources. <br />
    /// The implementers of this class do only need to provide a 
    /// class instance which implements the <see cref="System.Resources.IResourceReader"/> interface. <br />
    /// This class is abstract , and the <see cref="DefaultResourceLoader"/> serves as it's implementation class.
    /// </summary>
    public abstract class OptimizedResourceLoader : DefaultResourceLoader , IResourceLoader
    {
        private Dictionary<System.String, System.String> CommonSessionStrings;

        /// <summary>
        /// Default constructor that must be called from your inheriting class. <br />
        /// Failling to do so results in <see cref="NullReferenceException"/>s.
        /// </summary>
        protected OptimizedResourceLoader() : base() { CommonSessionStrings = new(50); }

        /// <summary>
        /// Disposes the allocated data used by the <see cref="OptimizedResourceLoader"/> class.
        /// </summary>
        /// <remarks>If you override this method , do not forget to call this one using base.Dispose() pattern.</remarks>
        public override void Dispose()
        {
            CommonSessionStrings?.Clear();
            CommonSessionStrings = null;
            base.Dispose();
        }

        /// <summary>
        /// Disposes asyncronously the allocated data used by the <see cref="OptimizedResourceLoader"/> class.
        /// </summary>
        /// <remarks>If you override this method , do not forget to call this one using base.DisposeAsync() pattern.</remarks>
        public override System.Threading.Tasks.ValueTask DisposeAsync() => new(System.Threading.Tasks.Task.Run(Dispose));

        /// <summary>
        /// Gets a string resource. Throws <see cref="ResourceNotFoundException"/> if not found.
        /// </summary>
        /// <param name="Name">The resource name to look up.</param>
        /// <returns>The string resource defined by <paramref name="Name"/>.</returns>
        /// <remarks>
        /// In order to provide optimization results , instead of each string resource is looked up multiple times , 
        /// it is saved to the process memory. <br />
        /// This has as a result to optimize some things and avoids calling a lot of times the resource lookup service. <br />
        /// Note: If that collection has more than 50 elements , these are deleted and new ones take their place.  <br />
        /// This is done for memory management reasons.
        /// </remarks>
        /// <exception cref="ResourceNotFoundException">The specified resource was not found.</exception>
        public override System.String GetStringResource(System.String Name)
        {
            // If the in-memory dictionary contains it , then return this instead.
            if (CommonSessionStrings.ContainsKey(Name)) { return CommonSessionStrings[Name]; }
            // Check if the dictionary holds more than 50 elements.
            // Clear it if the condition stands true.
            if (CommonSessionStrings.Count > 50) { CommonSessionStrings.Clear(); }
            foreach (DictionaryEntry D in read)
            {
                if (D.Key.ToString() == Name)
                {
                    if (D.Value.GetType() != typeof(System.String))
                    { throw GetResourceIncorrectType(Name, typeof(System.String), D.Value.GetType()); }
                    // Because the size of strings is small , create a fast random-access
                    // in-memory dictionary that contains all the hit occurences.
                    try { CommonSessionStrings.Add(D.Key.ToString(), D.Value.ToString()); } catch (ArgumentException) { }
                    return D.Value.ToString();
                }
            }
            throw GetResourceDoesNotExist(Name);
        }
    }
}
