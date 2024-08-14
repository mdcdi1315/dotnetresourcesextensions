using System;
using System.Collections;
using System.Collections.Generic;

namespace DotNetResourcesExtensions.Collections
{
    /// <summary>
    /// Represents an enumerator that is able to enumerate resource entries.
    /// </summary>
    public interface IResourceEntryEnumerator : IDictionaryEnumerator
    {
        /// <summary>
        /// Returns the resource entry at the current position of the enumerator.
        /// </summary>
        public IResourceEntry ResourceEntry { get; }
    }

    /// <summary>
    /// Represents an extended version of the <see cref="IResourceEntryEnumerator"/> that is able to return
    /// the true instances of the underlying resource entry defined in <typeparamref name="T"/>. <br />
    /// Because this interface inherits from <see cref="IResourceEntryEnumerator"/> , it is a good practice
    /// in the implementing class to explicitly define the <see cref="IResourceEntryEnumerator.ResourceEntry"/> 
    /// property to be bound to this <see cref="ResourceEntry"/> property. <br />
    /// Otherwise , <typeparamref name="T"/> only accepts types that are derived from <see cref="IResourceEntry"/>
    /// so there is no problem of doing the above stated.
    /// </summary>
    /// <typeparam name="T">The resource entry type that this enumerator will return. Must be a type that is derived from the <see cref="IResourceEntry"/> interface.</typeparam>
    public interface ISpecificResourceEntryEnumerator<T> : IResourceEntryEnumerator where T : notnull , IResourceEntry
    {
        /// <summary>
        /// Returns the resource entry of type <typeparamref name="T"/> at the current position of the enumerator.
        /// </summary>
        public new T ResourceEntry { get; }
    }
    
    /// <summary>
    /// Defines a collection enumerable that provides support for resource entries.
    /// </summary>
    public interface IResourceEntryEnumerable : IEnumerable<IResourceEntry>
    {
        /// <summary>
        /// Gets a resource entry enumerator.
        /// </summary>
        /// <returns>An object that implements the <see cref="IResourceEntryEnumerator"/> interface.</returns>
        public IResourceEntryEnumerator GetResourceEntryEnumerator();
    }

    /// <summary>
    /// Represents an abstract entry enumerator that acts as a wrapper for other enumerators which do not have support 
    /// or knowledge of <see cref="IResourceEntry"/> interface. <br />
    /// This class cannot be inherited.
    /// </summary>
    public sealed class ResourceEntryEnumerator : IResourceEntryEnumerator
    {
        private IDictionaryEnumerator entries;
        private IEnumerator<IResourceEntry> entries2;
        private REUsage usage;

        // Enumeration for determining which instance to retrieve.
        private enum REUsage : System.Byte { IDictionaryEnumerator , IGenericEnumerator }

        // Creates typed entries from existing dictionary entries , however ,
        // we could create directly the needed resource entries but there is the 
        // problem that the resource entry extensions would consider it as a 'Created Entry' and we do not really want that. 
        private class HiddenTypedResourceEntry : IResourceEntry
        {
            private readonly string name;
            private readonly object value;

            public HiddenTypedResourceEntry(string name, object value)
            {
                this.name = name;
                this.value = value;
            }

            public HiddenTypedResourceEntry(DictionaryEntry de)
            {
                name = de.Key.ToString();
                value = de.Value;
            }

            public string Name => name;

            public object Value => value;

            public Type TypeOfValue => value?.GetType();
        }

        /// <summary>
        /// Creates a new <see cref="ResourceEntryEnumerator"/> instance from the specified underlying dictionary enumerator.
        /// </summary>
        /// <param name="de">The dictionary enumerator to read resources from.</param>
        public ResourceEntryEnumerator(IDictionaryEnumerator de)
        {
            entries = de;
            usage = REUsage.IDictionaryEnumerator;
        }

        /// <summary>
        /// Creates a new <see cref="ResourceEntryEnumerator"/> instance from the specified underlying enumerator which returns <see cref="IResourceEntry"/> objects..
        /// </summary>
        /// <param name="enumerator">The enumerator to read resources from.</param>
        public ResourceEntryEnumerator(IEnumerator<IResourceEntry> enumerator)
        {
            entries2 = enumerator;
            usage = REUsage.IGenericEnumerator;
        }

        /// <summary>
        /// Returns a new instance of <see cref="IResourceEntry"/> suitable as a resource entry created from the current dictionary entry.
        /// </summary>
        public IResourceEntry ResourceEntry
        {
            get
            {
                switch (usage)
                {
                    case REUsage.IGenericEnumerator:
                        return (IResourceEntry)Current;
                    case REUsage.IDictionaryEnumerator:
                        return new HiddenTypedResourceEntry(entries.Entry);
                    default: 
                        return null;
                }
            }
        }

        /// <summary>
        /// Gets the resource entry at the current position of the enumerator.
        /// </summary>
        public object Current
        {
            get
            {
                switch (usage)
                {
                    case REUsage.IGenericEnumerator:
                        return entries2.Current;
                    case REUsage.IDictionaryEnumerator:
                        return entries.Current;
                    default:
                        return null;
                }
            }
        }

        /// <inheritdoc/>
        public object Key
        {
            get
            {
                switch (usage)
                {
                    case REUsage.IGenericEnumerator:
                        return ResourceEntry.Name;
                    case REUsage.IDictionaryEnumerator:
                        return entries.Key;
                    default:
                        return null;
                }
            }
        }

        /// <inheritdoc/>
        public object Value
        {
            get
            {
                switch (usage)
                {
                    case REUsage.IGenericEnumerator:
                        return ResourceEntry.Value;
                    case REUsage.IDictionaryEnumerator:
                        return entries.Value;
                    default:
                        return null;
                }
            }
        }

        /// <inheritdoc/>
        public DictionaryEntry Entry
        {
            get
            {
                switch (usage)
                {
                    case REUsage.IGenericEnumerator:
                        return new(ResourceEntry.Name , ResourceEntry.Value);
                    case REUsage.IDictionaryEnumerator:
                        return entries.Entry;
                    default:
                        return default;
                }
            }
        }

        /// <inheritdoc />
        public bool MoveNext()
        {
            switch (usage)
            {
                case REUsage.IGenericEnumerator:
                    return entries2.MoveNext();
                case REUsage.IDictionaryEnumerator:
                    return entries.MoveNext();
                default:
                    return false;
            }
        }

        /// <inheritdoc />
        public void Reset()
        {
            switch (usage)
            {
                case REUsage.IGenericEnumerator:
                    entries2.Reset();
                    break;
                case REUsage.IDictionaryEnumerator:
                    entries.Reset();
                    break;
            }
        }
    }

    /// <summary>
    /// Represents an enumerator that acts as a wrapper for <see cref="IEnumerator{T}"/> instances for interoperating when using
    /// a <see cref="IResourceEntryEnumerator"/>. <br />
    /// This permits the user to create only one enumerator implementation for cases that the <see cref="IResourceEntryEnumerable"/> is implemented.<br />
    /// For example , when you need a System.Collections.Generic.IEnumerator&lt;IResourceEntry&gt; , you use an instance of this class and then cast to IEnumerator to get it.
    /// </summary>
    public sealed class DualResourceEntryEnumerator : AbstractDualResourceEntryEnumerator , IResourceEntryEnumerator , 
        IEnumerator<IResourceEntry> , IEnumerator<DictionaryEntry> , 
        IEnumerator<KeyValuePair<System.String , System.Object>>
    {
        private IResourceEntryEnumerator en;

        /// <summary>
        /// Creates a new instance of <see cref="DualResourceEntryEnumerator"/> class with specified underlying enumerator implementation.
        /// </summary>
        /// <param name="en">The enumerator implementation to use.</param>
        public DualResourceEntryEnumerator(IResourceEntryEnumerator en) { this.en = en; }

        /// <inheritdoc />
        public override IResourceEntry ResourceEntry => en.ResourceEntry;

        /// <inheritdoc />
        public override DictionaryEntry Entry => en.Entry;

        /// <inheritdoc />
        public override object Key => en.Key;

        /// <inheritdoc />
        public override object Value => en.Value;

        /// <inheritdoc />
        public override bool MoveNext() => en.MoveNext();

        /// <inheritdoc />
        public override void Reset() => en.Reset();

        /// <summary>Disposes this enumerator instance.</summary>
        public override void Dispose() {
            en = null;
            base.Dispose();
        }
    }

    /// <summary>
    /// Provides an abstract implementation of <see cref="DualResourceEntryEnumerator"/> so as to use it in your own code.
    /// </summary>
    public abstract class AbstractDualResourceEntryEnumerator : IResourceEntryEnumerator ,
        IEnumerator<IResourceEntry>, IEnumerator<DictionaryEntry>,
        IEnumerator<KeyValuePair<System.String, System.Object>>
    {
        /// <summary>
        /// Default empty constructor. You might probably not even need this one.
        /// </summary>
        protected AbstractDualResourceEntryEnumerator() { }

        IResourceEntry IEnumerator<IResourceEntry>.Current => ResourceEntry;

        DictionaryEntry IEnumerator<DictionaryEntry>.Current => Entry;

        KeyValuePair<System.String, System.Object> IEnumerator<KeyValuePair<System.String, System.Object>>.Current
            => new(ResourceEntry.Name, ResourceEntry.Value);

        /// <inheritdoc />
        public abstract IResourceEntry ResourceEntry { get; }

        /// <inheritdoc />
        public abstract DictionaryEntry Entry { get; }

        /// <inheritdoc />
        public abstract System.Object Key { get; }

        /// <inheritdoc />
        public abstract System.Object Value { get; }

        /// <inheritdoc />
        public object Current => Entry;

        /// <inheritdoc />
        public abstract System.Boolean MoveNext();

        /// <inheritdoc />
        public abstract void Reset();

        /// <summary>By default , this method is empty. If you need to free resources , override this method and provide the disposal routines.</summary>
        public virtual void Dispose() { GC.SuppressFinalize(this); }
    }

    /// <summary>
    /// Provides a simplified abstract implementation of <see cref="DualResourceEntryEnumerator"/> so as to use it in your own code.
    /// </summary>
    public abstract class AbstractSimpleDualResourceEntryEnumerator : AbstractDualResourceEntryEnumerator
    {
        /// <summary>
        /// Default empty constructor. You might probably not even need this one.
        /// </summary>
        protected AbstractSimpleDualResourceEntryEnumerator() { }

        /// <inheritdoc />
        public override DictionaryEntry Entry => new(ResourceEntry.Name , ResourceEntry.Value);

        /// <inheritdoc />
        public override System.Object Key => ResourceEntry.Name;

        /// <inheritdoc />
        public override System.Object Value => ResourceEntry.Value;

        /// <summary>By default , this method is empty. If you need to free resources , override this method and provide the disposal routines.</summary>
        public override void Dispose() { base.Dispose(); }
    }

}