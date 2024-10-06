using System;
using System.Collections;
using System.Collections.Generic;

namespace DotNetResourcesExtensions.Collections
{
    /// <summary>
    /// Represents an enumerator that acts as a wrapper for <see cref="IEnumerator{T}"/> instances for interoperating when using
    /// a <see cref="IResourceEntryEnumerator"/>. <br />
    /// This permits the user to create only one enumerator implementation for cases that the <see cref="IResourceEntryEnumerable"/> is implemented.<br />
    /// For example , when you need a System.Collections.Generic.IEnumerator&lt;IResourceEntry&gt; , you use an instance of this class and then cast to IEnumerator to get it.
    /// </summary>
    public sealed class DualResourceEntryEnumerator : AbstractDualResourceEntryEnumerator
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
        public override void Dispose()
        {
            en = null;
            base.Dispose();
        }
    }

    /// <summary>
    /// Provides an abstract implementation of <see cref="DualResourceEntryEnumerator"/> so as to use it in your own code.
    /// </summary>
    public abstract class AbstractDualResourceEntryEnumerator : IDualResourceEntryEnumerator
    {
        /// <summary>
        /// Default empty constructor. You might probably not even need this one.
        /// </summary>
        protected AbstractDualResourceEntryEnumerator() { }

        IResourceEntry IEnumerator<IResourceEntry>.Current => ResourceEntry;

        DictionaryEntry IEnumerator<DictionaryEntry>.Current => Entry;

        IResourceEntry IDualResourceEntryEnumerator.Current => ResourceEntry;

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
        protected AbstractSimpleDualResourceEntryEnumerator() : base() { }

        /// <inheritdoc />
        public override DictionaryEntry Entry => ResourceEntry.AsDictionaryEntry();

        /// <inheritdoc />
        public override System.Object Key => ResourceEntry.Name;

        /// <inheritdoc />
        public override System.Object Value => ResourceEntry.Value;

        /// <summary>By default , this method is empty. If you need to free resources , override this method and provide the disposal routines.</summary>
        public override void Dispose() { base.Dispose(); }
    }

    /// <summary>
    /// Defines a simple abstract enumerator implementation to use for the <see cref="IDualResourceEntryWithCommentEnumerator"/>.
    /// </summary>
    public abstract class AbstractDualResourceEntryWithCommentEnumerator : IDualResourceEntryWithCommentEnumerator
    {
        /// <summary>
        /// Default empty constructor. You might probably not even need this one.
        /// </summary>
        protected AbstractDualResourceEntryWithCommentEnumerator() : base() { }

        IResourceEntry IEnumerator<IResourceEntry>.Current => ResourceEntry;

        DictionaryEntry IEnumerator<DictionaryEntry>.Current => ResourceEntry.AsDictionaryEntry();

        KeyValuePair<System.String, System.Object> IEnumerator<KeyValuePair<System.String, System.Object>>.Current
            => new(ResourceEntry.Name, ResourceEntry.Value);

        DictionaryEntry IDictionaryEnumerator.Entry => ResourceEntry.AsDictionaryEntry();

        IResourceEntry IResourceEntryEnumerator.ResourceEntry => ResourceEntry;

        IResourceEntryWithComment IEnumerator<IResourceEntryWithComment>.Current => ResourceEntry;

        DictionaryEntryWithComment IEnumerator<DictionaryEntryWithComment>.Current => Entry;

        IResourceEntryWithComment IDualResourceEntryWithCommentEnumerator.Current => ResourceEntry;

        IResourceEntry IDualResourceEntryEnumerator.Current => ResourceEntry;

        /// <summary>
        /// Returns the resource entry at the current position of the enumerator.
        /// </summary>
        public abstract IResourceEntryWithComment ResourceEntry { get; }

        /// <summary>
        /// Returns the resource entry at the current position of the enumerator.
        /// </summary>
        public DictionaryEntryWithComment Entry => ResourceEntry.AsDictionaryEntryWithComment();

        /// <inheritdoc />
        public System.Object Key => ResourceEntry.Name;

        /// <inheritdoc />
        public System.Object Value => ResourceEntry.Value;

        /// <inheritdoc />
        public System.Object Current => ResourceEntry.AsDictionaryEntry();

        /// <inheritdoc />
        public abstract System.Boolean MoveNext();

        /// <inheritdoc />
        public abstract void Reset();

        /// <summary>By default , this method is empty. If you need to free resources , override this method and provide the disposal routines.</summary>
        public virtual void Dispose() { GC.SuppressFinalize(this); }
    }

    /// <summary>
    /// Provides the base abstraction level for all dual resource entry enumerators. <br />
    /// Represents an enumerator that acts as a wrapper for <see cref="IEnumerator{T}"/> instances for interoperating when using
    /// a <see cref="IResourceEntryEnumerator"/>. <br />
    /// This permits the user to create only one enumerator implementation for cases that the <see cref="IResourceEntryEnumerable"/> is implemented. <br />
    /// In V2 this enumerator will be used as the base implementation for the <see cref="IResourceEnumerable"/> interface.
    /// </summary>
    public interface IDualResourceEntryEnumerator : IResourceEntryEnumerator,
        IEnumerator<IResourceEntry>, IEnumerator<DictionaryEntry>,
        IEnumerator<KeyValuePair<System.String, System.Object>>, IDisposable
    {
        /// <inheritdoc cref="IEnumerator{T}.Current"/>
        public new IResourceEntry Current { get; }
    }

    /// <summary>
    /// Provides the base abstraction level for all dual resource entry with comment enumerators. <br />
    /// It's purpose is to define even more interopability with other enumerator interfaces too!
    /// </summary>
    public interface IDualResourceEntryWithCommentEnumerator :
        ISpecificResourceEntryEnumerator<IResourceEntryWithComment>,
        IEnumerator<IResourceEntryWithComment>,
        IEnumerator<DictionaryEntryWithComment>,
        IDualResourceEntryEnumerator
    {
        /// <inheritdoc cref="IEnumerator{T}.Current"/>
        public new IResourceEntryWithComment Current { get; }
    }
}
