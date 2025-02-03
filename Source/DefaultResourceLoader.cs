
using System;
using System.Collections;
using System.Collections.Generic;

namespace DotNetResourcesExtensions
{
    /// <summary>
    /// Represents a default implementation of the <see cref="IResourceLoader"/> interface. <br />
    /// The implementers of this class do only need to provide a 
    /// class instance which implements the <see cref="System.Resources.IResourceReader"/> interface. <br />
    /// This class is abstract.
    /// </summary>
    public abstract class DefaultResourceLoader : IResourceLoader , IAsyncDisposable , IDisposable
    {
        /// <summary>
        /// Provides a convenient and default implementation of <see cref="Collections.IDualResourceEntryWithCommentEnumerator"/>
        /// so as to use it in your applications.
        /// </summary>
        protected sealed class DefaultDualResourceEnumerator : Collections.AbstractDualResourceEntryWithCommentEnumerator
        {
            private sealed class ResourceEntryWithComment : IResourceEntryWithComment
            {
                private System.String name, comment;
                private System.Object value;

                public ResourceEntryWithComment(System.String name, System.Object value)
                {
                    this.name = name;
                    this.value = value;
                }

                public ResourceEntryWithComment(DictionaryEntry entry) : this(entry.Key.ToString(), entry.Value) { }

                public ResourceEntryWithComment(System.String name , System.Object value , System.String comment) : this(name , value)
                {
                    this.comment = comment;
                }

                public System.String Name => name;

                public System.Object Value => value;

                public System.String Comment { get => comment; set => comment = value; }

                public System.Type TypeOfValue => value?.GetType();
            }

            private IDictionaryEnumerator enumerator;

            /// <summary>
            /// Creates a new instance of <see cref="DefaultDualResourceEnumerator"/> class by using the specified resource reader enumerator.
            /// </summary>
            /// <param name="enumerator">The enumerator instance to manage.</param>
            public DefaultDualResourceEnumerator(IDictionaryEnumerator enumerator)
            {
                this.enumerator = enumerator;
            }

            /// <inheritdoc/>
            public override System.Boolean MoveNext() => enumerator.MoveNext();

            /// <inheritdoc/>
            public override void Reset() => enumerator.Reset();

            /// <inheritdoc/>
            public override IResourceEntryWithComment ResourceEntry => new ResourceEntryWithComment(enumerator.Entry);

            /// <summary>
            /// Disposes the current enumerator implementation instance.
            /// </summary>
            public sealed override void Dispose() {
                enumerator = null;
                base.Dispose();
            }
        }

        /// <summary>
        /// In this field you must provide an instance of <see cref="System.Resources.IResourceReader"/> class
        /// before exiting the constructor!!
        /// </summary>
        protected System.Resources.IResourceReader read;

        /// <summary>
        /// Default constructor that must be called from your inheriting class.
        /// </summary>
        protected DefaultResourceLoader() { }

        /// <summary>
        /// Constructs a new instance of <see cref="ResourceNotFoundException"/> convenient for throwing 
        /// when a resource with the specified name was not found.
        /// </summary>
        /// <param name="ResName">The resource name that was not found.</param>
        /// <returns>A constructed <see cref="ResourceNotFoundException"/>.</returns>
        [System.Diagnostics.DebuggerHidden]
        protected static ResourceNotFoundException GetResourceDoesNotExist(System.String ResName) => new(ResName);

        /// <summary>
        /// Gets a new instance of <see cref="ResourceTypeMismatchException"/> convenient for throwing 
        /// type mismatch exceptions.
        /// </summary>
        /// <param name="ResName">The resource name</param>
        /// <param name="Found">The incorrect type that the resource was attempted to be retrieved</param>
        /// <param name="Expected">The correct type that the resource must be retrieved.</param>
        /// <returns>A constructed <see cref="ResourceTypeMismatchException"/>.</returns>
        [System.Diagnostics.DebuggerHidden]
        protected static ResourceTypeMismatchException GetResourceIncorrectType(System.String ResName,
            System.Type Found, System.Type Expected) => new(Found, Expected, ResName);

        /// <summary>
        /// Disposes the allocated data used by the <see cref="DefaultResourceLoader"/> class.
        /// </summary>
        /// <remarks>If you override this method , do not forget to call this one using base.Dispose() pattern.</remarks>
        public virtual void Dispose()
        {
            read?.Close();
            read?.Dispose();
            read = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes asyncronously the allocated data used by the <see cref="DefaultResourceLoader"/> class.
        /// </summary>
        /// <remarks>If you override this method , do not forget to call this one using base.DisposeAsync() pattern.</remarks>
        public virtual System.Threading.Tasks.ValueTask DisposeAsync() => new(System.Threading.Tasks.Task.Run(Dispose));

        /// <summary>
        /// Default implementation to call Dispose when the instance is casted to an IDisposable.
        /// </summary>
        void IDisposable.Dispose() => Dispose();

        /// <summary>
        /// Default implementation to call DisposeAsync when the instance is casted to an IAsyncDisposable.
        /// </summary>
        System.Threading.Tasks.ValueTask IAsyncDisposable.DisposeAsync() => DisposeAsync();

        /// <summary>
        /// Gets all the resource names that belong to the current instance.
        /// </summary>
        /// <returns>All the resource names defined in this instance.</returns>
        public IEnumerable<string> GetAllResourceNames()
        {
            foreach (DictionaryEntry ED in read) { yield return ED.Key.ToString(); }
        }

        /// <summary>
        /// Gets a string resource. Throws <see cref="ResourceNotFoundException"/> if not found.
        /// </summary>
        /// <param name="Name">The resource name to look up.</param>
        /// <returns>The string resource defined by <paramref name="Name"/>.</returns>
        /// <exception cref="ResourceNotFoundException">The specified resource was not found.</exception>
        public virtual System.String GetStringResource(System.String Name)
        {
            foreach (DictionaryEntry D in read)
            {
                if (D.Key.ToString() == Name)
                {
                    if (D.Value.GetType() != typeof(System.String))
                    { throw GetResourceIncorrectType(Name, typeof(System.String), D.Value.GetType()); }
                    return D.Value.ToString();
                }
            }
            throw GetResourceDoesNotExist(Name);
        }

        /// <summary>
        /// Gets all resource names which start with the contents of <paramref name="start"/> parameter.
        /// </summary>
        /// <param name="start">The string which will be used as a search filter.</param>
        /// <returns>All the resource names which start with <paramref name="start"/>.</returns>
        public IEnumerable<System.String> GetResourceNamesStartingWith(System.String start)
        {
            foreach (DictionaryEntry dg in read)
            {
                if (dg.Key.ToString().StartsWith(start)) { yield return dg.Key.ToString(); }
            }
        }

        /// <summary>
        /// Gets a resource of the specified type and returns it. 
        /// </summary>
        /// <typeparam name="T">The type of the resource to return.</typeparam>
        /// <param name="Name">The name of the resource to get.</param>
        /// <returns>The specified resource by it's <paramref name="Name"/> , casted to <typeparamref name="T"/>.</returns>
        /// <exception cref="ResourceNotFoundException">The specified resource was not found.</exception>
        /// <exception cref="ResourceTypeMismatchException">The specified resource does not have as a type the type passed in <typeparamref name="T"/>.</exception>
        public virtual T GetResource<T>(System.String Name) where T : notnull
        {
            foreach (DictionaryEntry D in read)
            {
                if (D.Key.ToString() == Name)
                {
                    if (D.Value.GetType() != typeof(T))
                    { throw GetResourceIncorrectType(Name, typeof(T), D.Value.GetType()); }
                    return (T)D.Value; // Always succeeds because the case that D.Value is different than T is handled above.
                }
            }
            throw GetResourceDoesNotExist(Name);
        }

        /// <summary>
        /// Get all the resource names where the <paramref name="Name"/> provided starts with them , plus itself.
        /// </summary>
        /// <param name="Name">The resource name to look up.</param>
        /// <returns>An enumerable with relative resource names.</returns>
        public IEnumerable<System.String> GetRelativeResources(System.String Name)
        {
            System.String inline;
            foreach (DictionaryEntry D in read)
            {
                inline = D.Key.ToString();
                if (inline.StartsWith(Name)) { yield return inline; }
                if (inline == Name) { yield return inline; }
            }
        }

        /// <summary>
        /// Gets a reususable and extendable resource entry enumerator. <br />
        /// By default the method will attempt to use the provided <see cref="Collections.IDualResourceEntryEnumerator"/> 
        /// implementation if found; otherwise the internal heuristics will take action. <br />
        /// This method is <see langword="virtual"/> only and only to support different scenarios , based on the usage case.
        /// </summary>
        /// <returns>A new instance of a resource entry enumerator capable to enumerate all resource entries.</returns>
        public virtual Collections.IDualResourceEntryEnumerator GetEnumerator()
        {
            IDictionaryEnumerator en = read.GetEnumerator();
            if (en is Collections.IDualResourceEntryEnumerator resent) { return resent; }
            return new DefaultDualResourceEnumerator(en);
        }

        /// <inheritdoc />
        [Obsolete("This method is not any longer useful and will always throw an exception." , true)]
        public ISimpleResourceEnumerator GetSimpleResourceEnumerator() => throw new NotSupportedException("This method is deprecated and will be removed in V3.");

        /// <inheritdoc />
        [Obsolete("This method is not any longer useful and will always throw an exception.", true)]
        public IAdvancedResourceEnumerator GetAdvancedResourceEnumerator() => throw new NotSupportedException("This method is deprecated and will be removed in V3.");
    }
}
